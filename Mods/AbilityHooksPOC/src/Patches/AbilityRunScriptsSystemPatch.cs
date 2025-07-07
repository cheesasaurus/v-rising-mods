using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace ProfuselyViolentProgression.WallopWarpers;

[HarmonyPatch]
public static class Patches
{

    public static EntityManager EntityManager => WorldUtil.Server.EntityManager;

    [HarmonyPatch(typeof(AbilityRunScriptsSystem), nameof(AbilityRunScriptsSystem.OnUpdate))]
    [HarmonyPrefix]
    public static void AbilityRunScriptsSystem_Prefix(AbilityRunScriptsSystem __instance)
    {
        ProcessQuery_OnCastStarted(__instance);
        // there are several other queries for OnBlahBlah that aren't dealt with in this POC
        ProcessQuery_OnPreCastEnded(__instance);
        ProcessQuery_OnPostCastEnded(__instance);
    }

    private static void ProcessQuery_OnCastStarted(AbilityRunScriptsSystem __instance)
    {
        var query = __instance._OnCastStartedQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<AbilityCastStartedEvent>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            var ev = events[i];
            OnCastStarted(entities[i], ev);
        }
    }

    private static void ProcessQuery_OnPreCastEnded(AbilityRunScriptsSystem __instance)
    {
        var query = __instance._OnPreCastEndedQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<AbilityPreCastEndedEvent>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            var ev = events[i];
            OnPreCastEnded(entities[i], ev);
        }
    }

    private static void ProcessQuery_OnPostCastEnded(AbilityRunScriptsSystem __instance)
    {
        var query = __instance._OnPostCastEndedQuery;
        var entities = query.ToEntityArray(Allocator.Temp);
        var events = query.ToComponentDataArray<AbilityPostCastEndedEvent>(Allocator.Temp);
        for (var i = 0; i < events.Length; i++)
        {
            var ev = events[i];
            OnPostCastEnded(entities[i], ev);
        }
    }

    private static void OnCastStarted(Entity entity, AbilityCastStartedEvent ev)
    {
        if (!EntityManager.HasComponent<PlayerCharacter>(ev.Character))
        {
            return;
        }
        var playerCharacter = EntityManager.GetComponentData<PlayerCharacter>(ev.Character);
        var abilityPrefabName = DebugUtil.LookupPrefabName(ev.Ability);
        var abilityGroupPrefabName = DebugUtil.LookupPrefabName(ev.AbilityGroup);
        LogUtil.LogInfo($"CastStarted.\n  character: {playerCharacter.Name}\n  ability: {abilityPrefabName}\n  abilityGroup: {abilityGroupPrefabName}");
    }

    private static void OnPreCastEnded(Entity entity, AbilityPreCastEndedEvent ev)
    {
        if (!EntityManager.HasComponent<PlayerCharacter>(ev.Character))
        {
            return;
        }
        var playerCharacter = EntityManager.GetComponentData<PlayerCharacter>(ev.Character);
        var abilityPrefabName = DebugUtil.LookupPrefabName(ev.Ability);
        var abilityGroupPrefabName = DebugUtil.LookupPrefabName(ev.AbilityGroup);
        LogUtil.LogInfo($"PreCastEnded.\n  character: {playerCharacter.Name}\n  ability: {abilityPrefabName}\n  abilityGroup: {abilityGroupPrefabName}");
    }

    private static void OnPostCastEnded(Entity entity, AbilityPostCastEndedEvent ev)
    {
        if (!EntityManager.HasComponent<PlayerCharacter>(ev.Character))
        {
            return;
        }
        var playerCharacter = EntityManager.GetComponentData<PlayerCharacter>(ev.Character);
        var abilityPrefabName = DebugUtil.LookupPrefabName(ev.Ability);
        var abilityGroupPrefabName = DebugUtil.LookupPrefabName(ev.AbilityGroup);
        LogUtil.LogInfo($"PostCastEnded.\n  Character: {playerCharacter.Name}\n  Ability: {abilityPrefabName}\n  AbilityGroup: {abilityGroupPrefabName}\n  WasInterrupted: {ev.WasInterrupted}");
    }

}