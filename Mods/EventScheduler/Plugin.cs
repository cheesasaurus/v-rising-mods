using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.EventScheduler;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("HookDOTS.API")]
[BepInDependency("gg.deca.VampireCommandFramework")]
public class Plugin : BasePlugin
{
    private Harmony _harmony;
    private HookDOTS.API.HookDOTS _hookDOTS;

    public override void Load()
    {
        LogUtil.Init(Log);

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        Core.Initialize(Log);

        CommandRegistry.RegisterAll();

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        CommandRegistry.UnregisterAssembly();
        Core.Dispose();
        _hookDOTS?.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }
    
}
