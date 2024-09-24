using System;
using Bloodstone.API;
using ProjectM;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Entities;
using Unity.Transforms;

namespace VRisingMods.Core.Item;


public static class ItemUtil {

    public static AddItemResponse GiveItemToPlayer(Entity character, PrefabGUID prefabGUID, int amount) {
        var gameDataSystem = VWorld.Server.GetExistingSystemManaged<GameDataSystem>();
		var addItemSettings = AddItemSettings.Create(VWorld.Server.EntityManager, gameDataSystem.ItemHashLookupMap);
		return InventoryUtilitiesServer.TryAddItem(addItemSettings, character, prefabGUID, amount);
    }

    public static bool TryDropItemFromInventory(Entity character, PrefabGUID prefabGUID, int amount) {
        var entityManager = VWorld.Server.EntityManager;
        var gameDataSystem = VWorld.Server.GetExistingSystemManaged<GameDataSystem>();
        var commandBuffer = VWorld.Server.GetExistingSystemManaged<EntityCommandBufferSystem>().CreateCommandBuffer();

        InventoryUtilities.TryGetMainInventoryEntity(entityManager, character, out var mainInventoryEntity);
        return InventoryUtilitiesServer.TryDropItem(entityManager, commandBuffer, gameDataSystem.ItemHashLookupMap, mainInventoryEntity, prefabGUID, amount);
    }

    public static bool TryDropNewItem(Entity character, PrefabGUID prefabGUID, int amount) {
        ServerGameManager serverGameManager = VWorld.Server.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager;
        serverGameManager.CreateDroppedItemEntity(character, prefabGUID, amount);
        return true;
    }

}