using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace ShardPolice;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
[BepInDependency("gg.deca.Bloodstone")]
[Bloodstone.API.Reloadable]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    public static ManualLogSource Logger;

    public override void Load()
    {
        // Plugin startup logic
        Logger = Log;
        LogUtil.Init(Log);
        ShardPoliceConfig.Init(Config);
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        // Harmony patching
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        // Register all commands in the assembly with VCF
        CommandRegistry.RegisterAll();
    }

    public override bool Unload()
    {
        CommandRegistry.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }

}
