using System.Collections.Generic;
using Stunlock.Core;
using Unity.Entities;
using VRisingMods.Core.Prefabs;
using VRisingMods.Core.Utilities;

namespace VRisingMods.Core.Buff;

public class ShardRelicBuffUtil {

    private static readonly PrefabGUID[] ShardBuffs = {
        ShardPrefabs.AB_Interact_UseRelic_Behemoth_Buff,
        ShardPrefabs.AB_Interact_UseRelic_Manticore_Buff,
        ShardPrefabs.AB_Interact_UseRelic_Monster_Buff,
        ShardPrefabs.AB_Interact_UseRelic_Paladin_Buff,
    };

    private static Dictionary<PrefabGUID, string> ShardNames = new Dictionary<PrefabGUID, string>(){
        { ShardPrefabs.AB_Interact_UseRelic_Behemoth_Buff, "Behemoth"},
        { ShardPrefabs.AB_Interact_UseRelic_Manticore_Buff, "Winged Horror"},
        { ShardPrefabs.AB_Interact_UseRelic_Monster_Buff, "Monster"},
        { ShardPrefabs.AB_Interact_UseRelic_Paladin_Buff, "Solarus"},
    };

    public static string ShardName(PrefabGUID prefabGUID) {
        if (ShardNames.TryGetValue(prefabGUID, out string shardName)) {
            return shardName;
        }
        return "unrecognized shard";
    }

    public static void GiveShardBuffsToPlayer(Entity character) {
        foreach (var shardBuff in ShardBuffs) {
            BuffUtil.GiveBuffToPlayer(character, shardBuff);
        }
    }

    public static bool TryRemoveShardBuffsFromPlayer(Entity character) {
        bool wasABuffRemoved = false;
        foreach (var shardBuff in ShardBuffs) {
            wasABuffRemoved |= BuffUtil.TryRemoveBuffFromPlayer(character, shardBuff);
        }
        return wasABuffRemoved;
    }

    public static bool TryRemoveShardBuffsFromPlayerExceptOne(Entity character, PrefabGUID keptBuff) {
        bool wasABuffRemoved = false;
        foreach (var shardBuff in ShardBuffs) {
            if (!shardBuff.Equals(keptBuff)) {
                wasABuffRemoved |= BuffUtil.TryRemoveBuffFromPlayer(character, shardBuff);
            }
        }
        return wasABuffRemoved;
    }

    public static bool IsShardBuffRelated(Entity entity) {
        var entityManager = WorldUtil.Server.EntityManager;
        if (!entityManager.HasComponent<PrefabGUID>(entity)) {
            return false;
        }
        var prefabGUID = entityManager.GetComponentData<PrefabGUID>(entity);
        foreach (var shardBuffGUID in ShardBuffs) {
            if (prefabGUID.Equals(shardBuffGUID)) {
                return true;
            }
        }
        return false;
    }
    
}
