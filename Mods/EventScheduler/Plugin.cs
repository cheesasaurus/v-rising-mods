using BepInEx;
using BepInEx.Unity.IL2CPP;
using cheesasaurus.VRisingMods.EventScheduler.Config;
using cheesasaurus.VRisingMods.EventScheduler.Repositories;
using HarmonyLib;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.EventScheduler;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("HookDOTS.API")]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    HookDOTS.API.HookDOTS _hookDOTS;
    EventRunner eventRunner;

    public override void Load()
    {
        // Plugin startup logic
        LogUtil.Init(Log);
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        var eventsConfig = EventsConfig.Init(MyPluginInfo.PLUGIN_GUID, "events.jsonc");
        var eventHistory = new EventHistoryRepository(MyPluginInfo.PLUGIN_GUID, "EventHistory.db");
        eventRunner = new EventRunner(eventsConfig, eventHistory);

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        Patches.BeforeChatMessageSystemUpdates += Tick;
    }

    public override bool Unload()
    {
        Patches.BeforeChatMessageSystemUpdates -= Tick;
        _hookDOTS?.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }

    public void Tick()
    {
        eventRunner.Tick();
    }
    
}
