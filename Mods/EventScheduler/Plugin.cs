using System;
using System.IO;
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
    FileSystemWatcher EventsConfigWatcher;
    IEventHistoryRepository EventHistory;
    EventRunner eventRunner;

    public override void Load()
    {
        LogUtil.Init(Log);

        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        _hookDOTS = new HookDOTS.API.HookDOTS(MyPluginInfo.PLUGIN_GUID, Log);
        _hookDOTS.RegisterAnnotatedHooks();

        ConfigSetUp();
        EventRunnerSetUp();

        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");
    }

    public override bool Unload()
    {
        EventRunnerTearDown();
        ConfigTearDown();
        _hookDOTS?.Dispose();
        _harmony?.UnpatchSelf();
        return true;
    }

    private void ConfigSetUp()
    {
        var filename = "events.jsonc";
        var filepath = EventsConfig.Filepath(MyPluginInfo.PLUGIN_GUID, filename);
        var dir = Path.GetDirectoryName(filepath);

        EventsConfig.CopyExampleIfNotExists(MyPluginInfo.PLUGIN_GUID, filename, "cheesasaurus.VRisingMods.EventScheduler.resources.example-events.jsonc");

        EventsConfigWatcher = new FileSystemWatcher(dir);
        EventsConfigWatcher.Filter = filename;
        EventsConfigWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
        EventsConfigWatcher.Changed += HandleConfigChanged;
        EventsConfigWatcher.EnableRaisingEvents = true;
    }

    private void ConfigTearDown()
    {
        EventsConfigWatcher.Changed -= HandleConfigChanged;
    }

    public void HandleConfigChanged(object sender, FileSystemEventArgs e)
    {
        LogUtil.LogMessage("EventScheduler config changed. Restarting the EventRunner.");
        try
        {
            EventRunnerTearDown();
            EventRunnerSetUp();
        }
        catch (Exception ex)
        {
            LogUtil.LogError(ex);
        }
    }

    private void EventRunnerSetUp()
    {
        try
        {
            EventsConfig = EventsConfig.Init(MyPluginInfo.PLUGIN_GUID, "events.jsonc");
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Error parsing events.jsonc: {ex}");
            return;
        }
        
        EventHistory = new EventHistoryRepository_JSON(MyPluginInfo.PLUGIN_GUID, "EventHistory.json");
        EventHistory.TryLoad();
        eventRunner = new EventRunner(EventsConfig, EventHistory);
        Hooks.BeforeChatMessageSystemUpdates += Tick;
        Hooks.BeforeWorldSave += Save;
    }

    private void EventRunnerTearDown()
    {
        try
        {
            Save();
        }
        catch (Exception ex)
        {
            Log.LogError($"Error saving. {ex}");
        }
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
