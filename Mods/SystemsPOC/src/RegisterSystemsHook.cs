using HarmonyLib;
using ProjectM;
using VRisingMods.Core.Utilities;

namespace SystemsPOC;

[HarmonyPatch]
public static class ServerWorldManagerPatch
{
    private static bool _initialized = false;

    [HarmonyPatch(typeof(ServerWorldManager), nameof(ServerWorldManager.BeginCreateServerWorld))]
    [HarmonyPrefix]
    public static void Postfix(ServerWorldManager __instance)
    {
        // this does not seem to be the correct time to try this
        // it also wouldn't work when hot-reloading plugins
        // DoStuff(__instance);
    }

    private static void DoStuff(ServerWorldManager __instance)
    {
        var world = __instance.World;
        LogUtil.LogInfo($"ServerWorldManagerPatch will add systems to world: {world.Name}");
        Plugin.RegisterSystems(world);
    }
}