using ProjectM;
using ProjectM.Scripting;
using Stunlock.Core;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace VRisingMods.Core.Item;


public static class ItemUtil {

    public static AddItemResponse GiveItemToPlayer(Entity character, PrefabGUID prefabGUID, int amount) {
        var gameDataSystem = WorldUtil.Server.GetExistingSystemManaged<GameDataSystem>();
		var addItemSettings = AddItemSettings.Create(WorldUtil.Server.EntityManager, gameDataSystem.ItemHashLookupMap);
		return InventoryUtilitiesServer.TryAddItem(addItemSettings, character, prefabGUID, amount);
    }

    public static bool TryDropItemFromInventory(Entity character, PrefabGUID prefabGUID, int amount) {
        var entityManager = WorldUtil.Server.EntityManager;
        var gameDataSystem = WorldUtil.Server.GetExistingSystemManaged<GameDataSystem>();
        var commandBuffer = WorldUtil.Server.GetExistingSystemManaged<EntityCommandBufferSystem>().CreateCommandBuffer();

        InventoryUtilities.TryGetMainInventoryEntity(entityManager, character, out var mainInventoryEntity);
        return InventoryUtilitiesServer.TryDropItem(entityManager, commandBuffer, gameDataSystem.ItemHashLookupMap, mainInventoryEntity, prefabGUID, amount);
    }

    public static bool TryDropNewItem(Entity character, PrefabGUID prefabGUID, int amount) {
        ServerGameManager serverGameManager = WorldUtil.Server.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager;
        serverGameManager.CreateDroppedItemEntity(character, prefabGUID, amount);
        return true;
    }

}