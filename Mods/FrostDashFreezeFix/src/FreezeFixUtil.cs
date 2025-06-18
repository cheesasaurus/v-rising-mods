using System;
using System.Collections.Generic;
using System.Linq;
using ProjectM;
using ProjectM.Scripting;
using ProjectM.Shared;
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
    public static Dictionary<Entity, Entity> ConsumeChillToFreezeThisTick = new(); // todo: could be multiple events per victim 


    private static EntityManager EntityManager = WorldUtil.Game.EntityManager;

    static PrefabGUID Frost_Vampire_Buff_Chill = new PrefabGUID(27300215);
    static PrefabGUID Frost_Vampire_Buff_Freeze = new PrefabGUID(612319955);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze = new PrefabGUID(1152422729);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Nova = new PrefabGUID(-36383536);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack = new PrefabGUID(-292495274);
    static PrefabGUID SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Recast = new PrefabGUID(774570130);

    static HashSet<PrefabGUID> PrefabsThatConsumeToFreeze = [
        //Frost_Vampire_Buff_Freeze, // todo: this doesn't consume to freeze. It is the effect created after the consume check
        SpellMod_Shared_Frost_ConsumeChillIntoFreeze,
        SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Nova,
        SpellMod_Shared_Frost_ConsumeChillIntoFreeze_OnAttack,
        SpellMod_Shared_Frost_ConsumeChillIntoFreeze_Recast
    ];

    // AB_Vampire_VeilOfFrost_TriggerBonusEffects PrefabGuid(-1688602321)


    public static void NewTickStarted()
    {
        TickCount++;
        CurrentTick_CallCount = 0;
        FrostDashAttackersThisTick.Clear();
        ChilledThisTick.Clear();
        ConsumeChillToFreezeThisTick.Clear();
    }

    public static void RecursiveUpdateStarting()
    {
        CurrentTick_CallCount++;
    }

    public static string RecursiveTickStamp {
        get => $"{TickCount}-{CurrentTick_CallCount}";
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
            LogUtil.LogWarning($"{RecursiveTickStamp} Going to spawn chill buff for something");
            //SystemPatchUtil.CancelJob(entity);
        }

        if (IsConsumeChillToFreezeBuff(entity))
        {
            ConsumeChillToFreezeThisTick.Add(entityToBuff, entity);
            LogUtil.LogWarning($"{RecursiveTickStamp} Will freeze something if chill consumed");
        }

        if (IsDirectFreezeBuff(entity))
        {
            LogUtil.LogWarning($"{RecursiveTickStamp} Going to spawn freeze buff for something");
            DebugUtil.LogComponentTypes(entity);

            if (EntityManager.HasBuffer<SpellModSetComponent>(entity))
            {
                LogUtil.LogInfo($"  spell mods:");
                var smsc = EntityManager.GetComponentData<SpellModSetComponent>(entity);
                var sm = smsc.SpellMods;
                LogUtil.LogInfo($"    count:{sm.Count}");
                LogUtil.LogInfo($"    0:{LookupPrefabName(sm.Mod0.Id)}");
                LogUtil.LogInfo($"    1:{LookupPrefabName(sm.Mod1.Id)}");
                LogUtil.LogInfo($"    2:{LookupPrefabName(sm.Mod2.Id)}");
                LogUtil.LogInfo($"    3:{LookupPrefabName(sm.Mod3.Id)}");
                LogUtil.LogInfo($"    4:{LookupPrefabName(sm.Mod4.Id)}");
                LogUtil.LogInfo($"    5:{LookupPrefabName(sm.Mod5.Id)}");
                LogUtil.LogInfo($"    6:{LookupPrefabName(sm.Mod6.Id)}");
                LogUtil.LogInfo($"    7:{LookupPrefabName(sm.Mod7.Id)}");
                LogUtil.LogInfo("     ----");
            }

            if (HasSpellModToConsumeChillIntoFreeze(entity))
            {
                LogUtil.LogError("Found a sus freeze buff");
                ConsumeChillToFreezeThisTick.Add(entityToBuff, entity);
            }
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
        LogUtil.LogInfo($"{RecursiveTickStamp} not a freeze buff: {LookupPrefabName(prefabGUID)}");
        return false;
    }

    public static bool IsDirectFreezeBuff(Entity entity)
    {
        if (!EntityManager.HasComponent<PrefabGUID>(entity))
        {
            return false;
        }
        var prefabGUID = EntityManager.GetComponentData<PrefabGUID>(entity);
        return prefabGUID.Equals(Frost_Vampire_Buff_Freeze);
    }

    public static IEnumerable<Entity> VictimsOfInstaFreeze()
    {
        var maybeFreezeThisTick = ConsumeChillToFreezeThisTick.Keys.ToHashSet();
        return ChilledThisTick.Intersect(maybeFreezeThisTick);
    }

    public static void CancelBadFreezeEvents()
    {
        var victims = VictimsOfInstaFreeze();
        LogUtil.LogError($"{RecursiveTickStamp} Cancelling {victims.Count()} bad freezes");
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

    public static bool HasSpellModToConsumeChillIntoFreeze(Entity entity)
    {
        if (!EntityManager.HasBuffer<SpellModSetComponent>(entity)) {
            return false;
        }
        var smsc = EntityManager.GetComponentData<SpellModSetComponent>(entity);
        var sm = smsc.SpellMods;
        SpellMod[] spellMods = { sm.Mod0, sm.Mod1, sm.Mod2, sm.Mod3, sm.Mod4, sm.Mod5, sm.Mod6, sm.Mod7 };
        foreach (var mod in spellMods)
        {
            if (PrefabsThatConsumeToFreeze.Contains(mod.Id)) {
                return true;
            }
        }
        return false;
    }

}