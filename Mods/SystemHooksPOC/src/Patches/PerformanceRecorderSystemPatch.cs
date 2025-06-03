using System;
using HarmonyLib;
using Il2CppInterop.Runtime;
using ProjectM.Gameplay.Systems;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace SystemHooksPOC.Patches;

[HarmonyPatch]
public unsafe class PerformanceRecorderSystemPatch
{
    static Type HookedSystemType = typeof(DealDamageSystem);
    private static SystemTypeIndex _hookedSystemTypeIndex;

    static bool _initialized = false;
    static bool _wasStaticRecordingEnabled = false;

    [HarmonyPatch(typeof(PerformanceRecorderSystem), nameof(PerformanceRecorderSystem.OnUpdate))]
    [HarmonyPrefix]
    static bool PerformanceRecorderSystem_OnUpdate_Prefix(PerformanceRecorderSystem __instance)
    {
       if (_initialized) {
            return false;
        }
        _wasStaticRecordingEnabled = PerformanceRecorderSystem.StaticRecordingEnabled;
        PerformanceRecorderSystem.StaticRecordingEnabled = true;
        _initialized = true;

        _hookedSystemTypeIndex = TypeManager.GetSystemTypeIndex(Il2CppType.From(HookedSystemType));
        if (_hookedSystemTypeIndex == SystemTypeIndex.Null)
        {
            LogUtil.LogError($"null sytem type index for {HookedSystemType}");
        }
        else
        {
            LogUtil.LogInfo($"hooked system: {TypeManager.GetSystemType(_hookedSystemTypeIndex).FullName}");
        }
        // unless desired, Disable PerformanceRecorderSystem.OnUpdate() call as it would do actual data recording and log output.
        return _wasStaticRecordingEnabled;
    }

    private static SystemTypeIndex DealDamageSystem;

    [HarmonyPatch(typeof(PerformanceRecorderSystem), nameof(PerformanceRecorderSystem.StartSystem))]
    [HarmonyPrefix]
    static void PerformanceRecorderSystem_StartSystem_Prefix(SystemState* systemState)
    {
        if (systemState->m_SystemTypeIndex == _hookedSystemTypeIndex)
        {
            //LogUtil.LogInfo($"Updating {HookedSystemType}! (prefix)");
        }
    }

    [HarmonyPatch(typeof(PerformanceRecorderSystem), nameof(PerformanceRecorderSystem.EndSystem))]
    [HarmonyPostfix]
    static void PerformanceRecorderSystem_EndSystem_Postfix(SystemState* systemState)
    {
        if (systemState->m_SystemTypeIndex == _hookedSystemTypeIndex)
        {
            //LogUtil.LogInfo($"Updating {HookedSystemType}! (postfix)");
        }
    }

}