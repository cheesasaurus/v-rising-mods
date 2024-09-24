using System.Collections.Generic;
using Bloodstone.API;
using ProjectM;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Item;
using VRisingMods.Core.Prefabs;

namespace ShardPolice.Utils;

public static class ShardItemUtil {

    private static readonly PrefabGUID[] ShardItems = {
        ShardPrefabs.Item_Building_Relic_Behemoth,
        ShardPrefabs.Item_Building_Relic_Manticore,
        ShardPrefabs.Item_Building_Relic_Monster,
        ShardPrefabs.Item_Building_Relic_Paladin,
    };

    private static readonly Dictionary<PrefabGUID, PrefabGUID> ShardItemForBuilding = new Dictionary<PrefabGUID, PrefabGUID>() {
        { ShardPrefabs.TM_Relic_SoulShard_Behemoth, ShardPrefabs.Item_Building_Relic_Behemoth },
        { ShardPrefabs.TM_Relic_SoulShard_Manticore, ShardPrefabs.Item_Building_Relic_Manticore },
        { ShardPrefabs.TM_Relic_SoulShard_Monster, ShardPrefabs.Item_Building_Relic_Monster },
        { ShardPrefabs.TM_Relic_SoulShard_Paladin, ShardPrefabs.Item_Building_Relic_Paladin },
    };

    public static void PrepareShardItemsToDespawn() {
        var entityManager = VWorld.Server.EntityManager;
        var query = entityManager.CreateEntityQuery(new ComponentType[]{
            ComponentType.ReadOnly<Relic>(),
            ComponentType.ReadOnly<LifeTime>(),
        });
        foreach (var entity in query.ToEntityArray(Allocator.Temp)) {
            PrepareShardItemToDespawn(entity);
        }
    }

    public static void RemovePlacedShardsAndDropNearCharacterToDespawn(Entity character) {
        var entityManager = VWorld.Server.EntityManager;
        var query = entityManager.CreateEntityQuery(new ComponentType[]{
            ComponentType.ReadOnly<Relic>(),
            ComponentType.ReadOnly<BlueprintData>(),
        });

        foreach (var shardBuildingEntity in query.ToEntityArray(Allocator.Temp)) {
            entityManager.TryGetComponentData<PrefabGUID>(shardBuildingEntity, out var buildingPrefabGUID);
            if (ShardItemForBuilding.TryGetValue(buildingPrefabGUID, out var itemPrefabGUID)) {
                if (ItemUtil.TryDropNewItemWithCustomLifetime(character, itemPrefabGUID, 1, 1)) {
                    DestroyUtility.Destroy(entityManager, shardBuildingEntity, DestroyDebugReason.Consume);
                }
            }
        }
    }

    private static void PrepareShardItemToDespawn(Entity entity) {
        var entityManager = VWorld.Server.EntityManager;
        if (entityManager.TryGetComponentData<LifeTime>(entity, out var lifeTime)) {
            lifeTime.Duration = 0;
            entityManager.SetComponentData(entity, lifeTime);
        }
    }

}
