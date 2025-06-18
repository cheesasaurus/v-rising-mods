using System.Collections.Generic;
using System.Linq;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace FrostDashFreezeFix;

public static class FreezeFixUtil
{
    public static int TickCount = 0;
    public static int CurrentTick_CallCount = 0;
    public static ISet<Entity> FrostDashAttackersThisTick = new HashSet<Entity>();
    public static HashSet<Entity> ChilledThisTick = new();
    public static Dictionary<Entity, Entity> ConsumeChillToFreezeThisTick = new();


    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;

    static PrefabGUID Frost_Vampire_Buff_Chill = new PrefabGUID(27300215);
    static PrefabGUID Frost_Vampire_Buff_Freeze = new PrefabGUID(612319955);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze = new PrefabGUID(1152422729);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Nova = new PrefabGUID(-36383536);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack = new PrefabGUID(-292495274);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Recast = new PrefabGUID(774570130);

    static HashSet<PrefabGUID> PrefabsThatConsumeToFreeze = [
        Frost_Vampire_Buff_Freeze, // todo: this doesn't consume to freeze. It is the effect created after the consume check
        SpellMod_Shared_Frost_ConsumeChillIntoFreeze,
        SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Nova,
        SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack,
        SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Recast
    ];

    // AB_Vampire_VeilOfFrost_TriggerBonusEffects


    public static void NewTickStarted()
    {
        TickCount++;
        CurrentTick_CallCount = 0;
        FrostDashAttackersThisTick.Clear();
        ChilledThisTick.Clear();
        ConsumeChillToFreezeThisTick.Clear();
    }

    public static void BuffWillBeSpawned(Entity entity)
    {
        var entityOwner = EntityManager.GetComponentData<EntityOwner>(entity);
        var entityToBuff = entityOwner.Owner;
        /* todo: maybe only do this to players, for performance?
        if (!EntityManager.HasComponent<PlayerCharacter>(buffedCharacter))
        {
            return;
        }
        var playerCharacter = EntityManager.GetComponentData<PlayerCharacter>(EntityToBuff);
        */

        if (IsChillBuff(entity))
        {
            ChilledThisTick.Add(entityToBuff);
            LogUtil.LogWarning($"Going to spawn chill buff for something");
            //SystemPatchUtil.CancelJob(entity);
        }

        if (IsConsumeChillToFreezeBuff(entity))
        {
            ConsumeChillToFreezeThisTick.Add(entityToBuff, entity);
            LogUtil.LogWarning($"Will freeze something if chill consumed");
        }

    }

    public static bool IsChillBuff(Entity entity)
    {
        if (!EntityManager.HasComponent<PrefabGUID>(entity))
        {
            return false;
        }
        var prefabGUID = EntityManager.GetComponentData<PrefabGUID>(entity);
        return prefabGUID.Equals(Frost_Vampire_Buff_Chill);
    }

    public static bool IsConsumeChillToFreezeBuff(Entity entity)
    {
        if (!EntityManager.HasComponent<PrefabGUID>(entity))
        {
            return false;
        }
        var prefabGUID = EntityManager.GetComponentData<PrefabGUID>(entity);
        //return PrefabsThatConsumeToFreeze.Contains(prefabGUID); // not sure how equality is checked. do they need to be the same ref?
        foreach (var freezeGuid in PrefabsThatConsumeToFreeze)
        {
            if (prefabGUID.Equals(freezeGuid))
            {
                return true;
            }
        }
        LogUtil.LogInfo($"not a freeze buff: {LookupPrefabName(prefabGUID)}");
        return false;


    }

    public static IEnumerable<Entity> VictimsOfInstaFreeze()
    {
        var maybeFreezeThisTick = ConsumeChillToFreezeThisTick.Keys.ToHashSet();
        return ChilledThisTick.Intersect(maybeFreezeThisTick);
    }

    public static void CancelBadFreezeEvents()
    {
        var victims = VictimsOfInstaFreeze();
        LogUtil.LogError($"Cancelling {victims.Count()} bad freezes");
        foreach (var victim in victims)
        {
            var ev = ConsumeChillToFreezeThisTick[victim];
            SystemPatchUtil.CancelJob(ev);
        }
    }
    
    public static string LookupPrefabName(PrefabGUID prefabGuid)
    {
        var prefabCollectionSystem = WorldUtil.Game.GetExistingSystemManaged<PrefabCollectionSystem>();
        var prefabLookupMap = prefabCollectionSystem._PrefabLookupMap;
        if (prefabLookupMap.GuidToEntityMap.ContainsKey(prefabGuid))
        {
            return $"{prefabLookupMap.GetName(prefabGuid)} PrefabGuid({prefabGuid.GuidHash})";
        }
        return $"GUID Not Found {prefabGuid._Value}";
    }
    


}