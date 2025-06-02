using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Systems;
using ProjectM.Shared;
using ProjectM.WeaponCoating;
using Stunlock.Core;
using Stunlock.Network;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Utilities;


namespace FrostDashFreezeFix.Patches;

[HarmonyPatch]
internal static class FreezeFixPatch2
{
    static ulong debugId = 0;

    // maybe something with BuffSystem_Spawn_Server? maybe
    // maybe something with TravelBuffSystem? don't think so
    // maybe something with TravelBuffRegisterSystem? don't think so
    // maybe something with ApplyBuffOnSpawnSystem? don't think so
    // maybe something with DestroyBuffOnDamageTakenSystem? don't think so
    // AdditionalInteractBuffComponentDestroySystem? don't think so
    // Spawn_TravelBuffSystem? don't think so
    // Spawn_DashSystem? don't think so
    // BuffDebugSystem? looks promising but not sure if this is the correct place
    // CreateGameplayEventOnOwnerSpellHitConsumedSystem? this does something when cold snap is triggered but not when the dash attack hits
    // TODO: Apply_BuffModificationsSystem_Server? this has a bunch of queries, will come back to this later.
    // AdditionalInteractBuffComponentSpawnSystem? don't think so
    // 
    // 
    [HarmonyPatch(typeof(AdditionalInteractBuffComponentSpawnSystem), nameof(AdditionalInteractBuffComponentSpawnSystem.OnUpdate))]
    [HarmonyPrefix]
    static void OnUpdatePrefix(AdditionalInteractBuffComponentSpawnSystem __instance)
    {
        //LogUtil.LogInfo("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX");
        //DebugStuff(__instance);
    }

    static void DebugStuff(AdditionalInteractBuffComponentSpawnSystem  __instance)
    {
        var entities0 = __instance.__query_343062750_0.ToEntityArray(Allocator.Temp);
        LogUtil.LogInfo($"[entities0]------------------------------------");
        foreach (var entity in entities0)
        {
            DebugUtil.LogComponentTypes(entity);
        }
    }

}