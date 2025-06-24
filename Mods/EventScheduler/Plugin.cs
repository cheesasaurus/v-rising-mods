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
    IEventHistoryRepository EventHistory;
    EventRunner eventRunner;

    public override void Load()
    {
        LogUtil.Init(Log);

        var eventsConfig = EventsConfig.Init(MyPluginInfo.PLUGIN_GUID, "events.jsonc");
        EventHistory = new EventHistoryRepository_JSON(MyPluginInfo.PLUGIN_GUID, "EventHistory.json");
        EventHistory.TryLoad();
        eventRunner = new EventRunner(eventsConfig, EventHistory);

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        Hooks.BeforeChatMessageSystemUpdates += Tick;
        Hooks.BeforeWorldSave += Save;

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        Save();

        Hooks.BeforeChatMessageSystemUpdates -= Tick;
        Hooks.BeforeWorldSave -= Save;
        
        _hookDOTS?.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }

    public void Tick()
    {
        eventRunner.Tick();
    }

    public void Save()
    {
        LogUtil.LogDebug("Saving EventHistory");
        EventHistory?.TrySave();
    }
    
}
