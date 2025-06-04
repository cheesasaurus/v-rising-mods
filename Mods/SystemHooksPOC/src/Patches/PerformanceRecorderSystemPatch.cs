using HarmonyLib;
using Unity.Entities;

namespace SystemHooksPOC.Patches;

[HarmonyPatch]
public unsafe class PerformanceRecorderSystemPatch
{
    static bool _initialized = false;
    static bool _wasStaticRecordingEnabled = false;

    // todo: maybe there's a method that harmony will call when unpatching. so we don't have to call it ourselves
    internal static void UnInitialize()
    {
        if (!_initialized)
        {
            return;
        }
        // PerformanceRecorderSystem.StaticRecordingEnabled = _wasStaticRecordingEnabled; // todo: go back to this after fixing initial plugin setup to only happen after type manager indexes are ready
        PerformanceRecorderSystem.StaticRecordingEnabled = false;
        _wasStaticRecordingEnabled = false;
        _initialized = false;
    }

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
        // unless desired, Disable PerformanceRecorderSystem.OnUpdate() call as it would do actual data recording and log output.
        return _wasStaticRecordingEnabled;
    }

    private static SystemTypeIndex DealDamageSystem;

    [HarmonyPatch(typeof(PerformanceRecorderSystem), nameof(PerformanceRecorderSystem.StartSystem))]
    [HarmonyPrefix]
    static void PerformanceRecorderSystem_StartSystem_Prefix(SystemState* systemState)
    {
        HookManager.HandleSystemUpdatePrefix(systemState);
    }

    [HarmonyPatch(typeof(PerformanceRecorderSystem), nameof(PerformanceRecorderSystem.EndSystem))]
    [HarmonyPostfix]
    static void PerformanceRecorderSystem_EndSystem_Postfix(SystemState* systemState)
    {
        HookManager.HandleSystemUpdatePostfix(systemState);
    }

}