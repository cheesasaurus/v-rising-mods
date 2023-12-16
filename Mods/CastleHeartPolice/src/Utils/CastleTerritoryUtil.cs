using Bloodstone.API;
using CastleHeartPolice.Models;
using Il2CppInterop.Runtime;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Terrain;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace CastleHeartPolice.Utils;

public static class CastleTerritoryUtil {

    // One unit on the block grid is equivalent to 5 units on the world grid
    private static int BlockSize = 5;

    // I have no idea why, but the block coordinates origin and world coordinates origin don't exactly line up
    private static int2 BlockOffsetFromWorld = new int2(639, 639);

    public static int2 BlockCoordinatesFromWorldPosition(float3 worldPos) {
        var intWorldPos = new int2((int)worldPos.x, (int)worldPos.z);
        return (intWorldPos / BlockSize) + BlockOffsetFromWorld;
    }

    public static bool TryFindTerritoryContaining(float3 worldPos, out CastleTerritoryInfo territoryInfo) {
        var blockCoords = BlockCoordinatesFromWorldPosition(worldPos);
        float2 worldPos2 = worldPos.xz;
        var entityManager = VWorld.Server.EntityManager;
        
        var mapZoneCollectionSystem = VWorld.Server.GetExistingSystem<MapZoneCollectionSystem>();
        var mapZoneCollection = mapZoneCollectionSystem.GetMapZoneCollection();
        foreach (var spatialZone in mapZoneCollection._MapZoneLookup.GetValueArray(Allocator.Temp)) {
            if ((MapZoneFlags.CastleTerritory & spatialZone.ZoneFlags) == 0) {
                // not a castle territory
                continue;
            }

            // rough check (bounding rectangle, sometimes nearby territories' rectangles overlap)
            if (!spatialZone.WorldBounds.Contains(worldPos2)) {
                continue;
            }
            
            // detailed check (all the blocks where a castle floor could be placed. never overlaps with another territory)
            var blocks = entityManager.GetBuffer<CastleTerritoryBlocks>(spatialZone.ZoneEntity);
            foreach (var block in blocks) {
                if (block.BlockCoordinate.Equals(blockCoords)) {
                    var castleTerritory = entityManager.GetComponentData<CastleTerritory>(spatialZone.ZoneEntity);
                    territoryInfo = new CastleTerritoryInfo() {
                        TerritoryId = castleTerritory.CastleTerritoryIndex,
                        Entity = spatialZone.ZoneEntity,
                        CastleTerritory = castleTerritory,
                        BlockCount = blocks.Length,
                    };
                    return true;
                }
            }
        }
        territoryInfo = default;
        return false;
    }

    public static bool TryFindTerritoryOfCastleHeart(Entity heartEntity, out CastleTerritoryInfo territoryInfo) {
        var entityManager = VWorld.Server.EntityManager;
        var castleHeart = entityManager.GetComponentData<CastleHeart>(heartEntity);
        var castleTerritory = entityManager.GetComponentData<CastleTerritory>(castleHeart.CastleTerritoryEntity);

        var mapZoneCollectionSystem = VWorld.Server.GetExistingSystem<MapZoneCollectionSystem>();
        var mapZoneCollection = mapZoneCollectionSystem.GetMapZoneCollection();
        
        if (mapZoneCollection._MapZoneLookup.TryGetValue(castleHeart.CastleTerritoryId, out var spatialZone)) {
            var blocks = entityManager.GetBuffer<CastleTerritoryBlocks>(spatialZone.ZoneEntity);
            territoryInfo = new CastleTerritoryInfo() {
                TerritoryId = castleTerritory.CastleTerritoryIndex,
                Entity = castleHeart.CastleTerritoryEntity,
                CastleTerritory = castleTerritory,
                BlockCount = blocks.Length,
            };
            return true;
        }
        territoryInfo = default;
        return false;
    }

}
