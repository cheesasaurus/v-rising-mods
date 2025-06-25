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

    EventsConfig EventsConfig;
    IEventHistoryRepository EventHistory;
    EventRunner eventRunner;

    public override void Load()
    {
        LogUtil.Init(Log);

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        // todo: reload when config changed.
        InitConfig();
        EventRunnerSetUp();

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        EventRunnerTearDown();
        _hookDOTS?.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }

    private void InitConfig()
    {
        var filename = "events.jsonc";
        EventsConfig.CopyExampleIfNotExists(MyPluginInfo.PLUGIN_GUID, filename, "cheesasaurus.VRisingMods.EventScheduler.resources.example-events.jsonc");
        EventsConfig = EventsConfig.Init(MyPluginInfo.PLUGIN_GUID, filename);
    }

    private void EventRunnerSetUp()
    {
        EventHistory = new EventHistoryRepository_JSON(MyPluginInfo.PLUGIN_GUID, "EventHistory.json");
        EventHistory.TryLoad();
        eventRunner = new EventRunner(EventsConfig, EventHistory);
        Hooks.BeforeChatMessageSystemUpdates += Tick;
        Hooks.BeforeWorldSave += Save;
    }

    private void EventRunnerTearDown()
    {
        Save();
        Hooks.BeforeChatMessageSystemUpdates -= Tick;
        Hooks.BeforeWorldSave -= Save;
    }

    public void Tick()
    {
        if (!WorldUtil.IsServerInitialized)
        {
            return;
        }
        eventRunner?.Tick();
    }

    public void Save()
    {
        LogUtil.LogDebug("Saving EventHistory");
        EventHistory?.TrySave();
    }
    
}
