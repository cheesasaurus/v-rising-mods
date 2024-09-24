using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using ProjectM.Shared;
using Stunlock.Core;
using Unity.Entities;

namespace VRisingMods.Core.Buff;

public class BuffUtil {
    public static void GiveBuffToPlayer(Entity character, PrefabGUID buffPrefabGUID) {
        var playerCharacter = VWorld.Server.EntityManager.GetComponentData<PlayerCharacter>(character);

		var fromCharacter = new FromCharacter()
		{
			User = playerCharacter.UserEntity,
			Character = character,
		};

        var buffEvent = new ApplyBuffDebugEvent()
		{
			BuffPrefabGUID = buffPrefabGUID
		};

        var debugEventsSystem = VWorld.Server.GetExistingSystemManaged<DebugEventsSystem>();
        debugEventsSystem.ApplyBuff(fromCharacter, buffEvent);
    }

    public static bool TryRemoveBuffFromPlayer(Entity character, PrefabGUID buffPrefabGUID) {
        var entityManager = VWorld.Server.EntityManager;
        var playerHasBuff = BuffUtility.TryGetBuff(entityManager, character, buffPrefabGUID, out var buffEntity);
        if (playerHasBuff) {
            DestroyUtility.Destroy(entityManager, buffEntity, DestroyDebugReason.TryRemoveBuff);
            return true;
        }
        return false;
    }

}