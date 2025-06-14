using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ScarletRCON.Shared;
using VRisingMods.Core.Utilities;

namespace ScarletRCON_SoftDep_AsyncPOC;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("markvaaz.ScarletRCON", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    Harmony _harmony;

    public override void Load()
    {
        // Plugin startup logic
        LogUtil.Init(Log);
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        // Harmony patching
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        RconCommandRegistrar.RegisterAll();
    }

    public override bool Unload()
    {
        RconCommandRegistrar.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }
}
