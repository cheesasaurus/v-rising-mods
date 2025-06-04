using HarmonyLib;
using Stunlock.Core;
using VRisingMods.Core.Utilities;

namespace SystemHooksPOC.Patches;

[HarmonyPatch]
public static class WorldBootrapPatch
{
    [HarmonyPatch(typeof(WorldBootstrapUtilities), nameof(WorldBootstrapUtilities.AddSystemsToWorld))]
    [HarmonyPostfix]
    public static void Initialize()
    {
        LogUtil.LogInfo("Game has bootsrapped. Worlds and systems now exist.");
        HookManager.Bus.TriggerGameReadyForRegistration();
    }
}

