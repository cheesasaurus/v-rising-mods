using System;
using Bloodstone.API;
using ProjectM;
using Unity.Entities;

namespace ShardPolice.Utils;


public static class ItemUtil {

    public static AddItemResponse GiveItemToPlayer(Entity character, PrefabGUID prefabGUID, int amount) {
        var gameDataSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();
		var addItemSettings = AddItemSettings.Create(VWorld.Server.EntityManager, gameDataSystem.ItemHashLookupMap);
		return InventoryUtilitiesServer.TryAddItem(addItemSettings, character, prefabGUID, amount);
    }

    public static bool TryDropItemFromInventory(Entity character, PrefabGUID prefabGUID, int amount) {
        var entityManager = VWorld.Server.EntityManager;
        var gameDataSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();
        var commandBuffer = VWorld.Server.GetExistingSystem<EntityCommandBufferSystem>().CreateCommandBuffer();

        InventoryUtilities.TryGetMainInventoryEntity(entityManager, character, out var mainInventoryEntity);
        return InventoryUtilitiesServer.TryDropItem(entityManager, commandBuffer, gameDataSystem.ItemHashLookupMap, mainInventoryEntity, prefabGUID, amount);
    }

    public static bool TryDropNewItemWithCustomLifetime(Entity character, PrefabGUID prefabGUID, int amount, int lifetimeSeconds) {
        if (lifetimeSeconds < 1) {
            throw new ArgumentOutOfRangeException("weird things would happen if lifetime is too short, probably because of the dropping animation");
        }
        var entityManager = VWorld.Server.EntityManager;
        var gameDataSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();

        var itemEntity = InventoryUtilitiesServer.CreateInventoryItemEntity(entityManager, gameDataSystem.ItemHashLookupMap, prefabGUID);
        if (entityManager.TryGetComponentData<LifeTime>(itemEntity, out var lifeTime)) {
            lifeTime.Duration = lifetimeSeconds;
            entityManager.SetComponentData(itemEntity, lifeTime);
        }
        else {
            // todo: could add new lifetime component, not sure if would work outside shards. doesn't matter right now.
            return false;
        }

        InventoryUtilitiesServer.CreateDropItem(entityManager, character, prefabGUID, amount, itemEntity);
        return true;
    }

}