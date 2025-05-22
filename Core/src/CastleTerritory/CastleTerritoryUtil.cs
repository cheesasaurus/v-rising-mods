using System;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Terrain;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using VRisingMods.Core.CastleTerritory.Models;
using VRisingMods.Core.Utilities;

namespace VRisingMods.Core.CastleTerritory;

public static class CastleTerritoryUtil {

    // One unit on the block grid is equivalent to 5 units on the world grid
    private static int BlockSize = 5;

    // I have no idea why, but the block coordinates origin and world coordinates origin don't exactly line up
    private static int2 BlockOffsetFromWorld = new int2(639, 639);

    // Two units on the territory WorldBounds grid is equivalent to 1 unit on the world grid
    private static int TerritoryWorldBoundsScale = 2;

    // I have no idea why, but the territory WorldBounds origin and world coordinates origin don't exactly line up
    private static int2 TerritoryWorldBoundsOffsetFromWorld = new int2(3200, 3200);

    public static int2 BlockCoordinatesFromWorldPosition(float3 worldPos) {
        var intWorldPos = new int2((int)worldPos.x, (int)worldPos.z);
        return (intWorldPos / BlockSize) + BlockOffsetFromWorld;
    }

    public static float2 TerritoryWorldBoundsCoordinatesFromWorldPosition(float3 worldPos) {
        var f2 = worldPos.xz;
        return (f2 + TerritoryWorldBoundsOffsetFromWorld) * TerritoryWorldBoundsScale;
    }

    public static bool TryFindTerritoryContaining(float3 worldPos, out CastleTerritoryInfo territoryInfo) {
        var blockCoords = BlockCoordinatesFromWorldPosition(worldPos);
        var worldPos2 = TerritoryWorldBoundsCoordinatesFromWorldPosition(worldPos);
        var entityManager = WorldUtil.Server.EntityManager;
        
        var mapZoneCollectionSystem = WorldUtil.Server.GetExistingSystemManaged<MapZoneCollectionSystem>();
        var mapZoneCollection = mapZoneCollectionSystem.GetMapZoneCollection();
        foreach (var spatialZone in mapZoneCollection.MapZoneLookup.GetValueArray(Allocator.Temp)) {
            if ((MapZoneFlags.CastleTerritory & spatialZone.ZoneFlags) == 0) {
                // not a castle territory
                continue;
            }

            // rough check (bounding rectangle, sometimes nearby territories' rectangles overlap)
            if (!IsPositionInBounds(worldPos2, spatialZone.WorldBounds)) {
                continue;
            }
            
            // detailed check (all the blocks where a castle floor could be placed. never overlaps with another territory)
            var blocks = entityManager.GetBuffer<CastleTerritoryBlocks>(spatialZone.ZoneEntity);
            foreach (var block in blocks) {
                if (block.BlockCoordinate.Equals(blockCoords)) {
                    var castleTerritory = entityManager.GetComponentData<ProjectM.CastleBuilding.CastleTerritory>(spatialZone.ZoneEntity);
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
        var entityManager = WorldUtil.Server.EntityManager;
        var castleHeart = entityManager.GetComponentData<CastleHeart>(heartEntity);
        var castleTerritory = entityManager.GetComponentData<ProjectM.CastleBuilding.CastleTerritory>(castleHeart.CastleTerritoryEntity);

        var mapZoneCollectionSystem = WorldUtil.Server.GetExistingSystemManaged<MapZoneCollectionSystem>();
        var mapZoneCollection = mapZoneCollectionSystem.GetMapZoneCollection();
        
        if (mapZoneCollection.MapZoneLookup.TryGetValue(castleHeart.CastleTerritoryId, out var spatialZone)) {
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

    // BoundsMinMax.Contains used to take care of this, but the signature changed in 1.0 and it seems to check something else now
    public static bool IsPositionInBounds(float2 worldPosition, BoundsMinMax worldBounds) {
        var xMin = Math.Min(worldBounds.Min.x, worldBounds.Max.x);
        var xMax = Math.Max(worldBounds.Min.x, worldBounds.Max.x);
        var yMin = Math.Min(worldBounds.Min.y, worldBounds.Max.y);
        var yMax = Math.Max(worldBounds.Min.y, worldBounds.Max.y);
        return xMin <= worldPosition.x && xMax >= worldPosition.x && yMin <= worldPosition.y && yMax >= worldPosition.y;
    }

}
