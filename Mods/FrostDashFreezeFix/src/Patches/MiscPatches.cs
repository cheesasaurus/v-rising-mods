using HarmonyLib;
using HookDOTS.API.Attributes;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.UI;
using ProjectM.WeaponCoating;
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

    // [EcsSystemUpdatePrefix(typeof(Spawn_TravelBuffSystem))]
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

}