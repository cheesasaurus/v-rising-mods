using HarmonyLib;
using HookDOTS.API.Attributes;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Systems;
using ProjectM.UI;
using ProjectM.WeaponCoating;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace FrostDashFreezeFix.Patches;


[HarmonyPatch]
public unsafe class MiscPatches
{
    [EcsSystemUpdatePrefix(typeof(RecursiveGroup), onlyWhenSystemRuns: false)]
    public static void UpdateTickCount()
    {
        FreezeFixUtil.NewTickStarted();

    }

    [HarmonyPatch(typeof(RecursiveGroup), nameof(RecursiveGroup.DoRecursiveUpdate))]
    [HarmonyPrefix]
    public static void TrackRecursiveUpdates(RecursiveGroup __instance)
    {
        FreezeFixUtil.RecursiveUpdateStarting();
    }

    //[EcsSystemUpdatePrefix(typeof(ApplyBuffOnSpawnSystem))]
    public static bool Something()
    {
        LogUtil.LogError("skipping something");
        return false;
    }

    [HarmonyPatch(typeof(Apply_BuffModificationsSystem_Server), nameof(Apply_BuffModificationsSystem_Server.OnUpdate))]
    [HarmonyPrefix]
    public static void Asdf(Apply_BuffModificationsSystem_Server __instance)
    {
        LogUtil.LogError("gonna buff");
    }

    [EcsSystemUpdatePostfix(typeof(HandleGameplayEventsRecursiveSystem))]
    public static void Blah()
    {
        LogUtil.LogInfo($"{FreezeFixUtil.RecursiveTickStamp} DealDamageSystem");
        var entityManager = WorldUtil.Server.EntityManager;
        var query = entityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<DealDamageEvent>(),
            },
        });

        var events = query.ToEntityArray(Allocator.Temp);
        foreach (var eventEntity in events)
        {
            //DebugUtil.LogComponentTypes(eventEntity);
            var dealDamage = entityManager.GetComponentData<DealDamageEvent>(eventEntity);
            FreezeFixUtil.EntityGotHitWithDamage(dealDamage.Target);
            //DebugUtil.LogComponentTypes(dealDamage.Target);
            //FreezeFixUtil.LogTargetsBuffs(dealDamage.Target);
        }
    }

}