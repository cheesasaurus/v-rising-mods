using System;
using System.IO;
using BepInEx.Logging;
using cheesasaurus.VRisingMods.EventScheduler.Config;
using cheesasaurus.VRisingMods.EventScheduler.Repositories;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.EventScheduler;

public static class Core
{
    public static bool IsInitialized { get; private set; } = false;

    private static ManualLogSource _log;
    public static EventsConfig EventsConfig { get; private set; }
    private static FileSystemWatcher _eventsConfigWatcher;
    private static IEventHistoryRepository _eventHistory;
    public static EventRunner EventRunner { get; private set; }

    public static void Initialize(ManualLogSource log)
    {
        IsInitialized = true;
        _log = log;
        ConfigSetUp();
        EventRunnerSetUp();
    }

    public static void Dispose()
    {
        if (!IsInitialized)
        {
            return;
        }
        IsInitialized = false;
        EventRunnerTearDown();
        ConfigTearDown();
    }

    private static void Save()
    {
        _log.LogDebug("Saving EventHistory");
        _eventHistory?.TrySave();
    }

    private static void ConfigSetUp()
    {
        var filename = "events.jsonc";
        var filepath = EventsConfig.Filepath(MyPluginInfo.PLUGIN_GUID, filename);
        var dir = Path.GetDirectoryName(filepath);

        EventsConfig.CopyExampleIfNotExists(MyPluginInfo.PLUGIN_GUID, filename, "cheesasaurus.VRisingMods.EventScheduler.resources.example-events.jsonc");

        _eventsConfigWatcher = new FileSystemWatcher(dir);
        _eventsConfigWatcher.Filter = filename;
        _eventsConfigWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.CreationTime;
        _eventsConfigWatcher.Changed += HandleConfigChanged;
        _eventsConfigWatcher.EnableRaisingEvents = true;
    }

    private static void ConfigTearDown()
    {
        _eventsConfigWatcher.Changed -= HandleConfigChanged;
    }

    private static void HandleConfigChanged(object sender, FileSystemEventArgs e)
    {
        LogUtil.LogMessage("EventScheduler config changed. Restarting the EventRunner.");
        try
        {
            EventRunnerTearDown();
            EventRunnerSetUp();
        }
        catch (Exception ex)
        {
            _log.LogError(ex);
        }
    }

    private static void EventRunnerSetUp()
    {
        try
        {
            EventsConfig = EventsConfig.Init(MyPluginInfo.PLUGIN_GUID, "events.jsonc");
        }
        catch (Exception ex)
        {
            _log.LogError($"Error parsing events.jsonc: {ex}");
            return;
        }

        _eventHistory = new EventHistoryRepository_JSON(MyPluginInfo.PLUGIN_GUID, "EventHistory.json");
        _eventHistory.TryLoad();
        EventRunner = new EventRunner(_log, EventsConfig, _eventHistory);
        Hooks.BeforeChatMessageSystemUpdates += RunBeforeChatMessageSystemUpdates;
        Hooks.AfterChatMessageSystemUpdates += RunAfterChatMessageSystemUpdates;
        Hooks.BeforeWorldSave += Save;
    }

    private static void EventRunnerTearDown()
    {
        try
        {
            Save();
        }
        catch (Exception ex)
        {
            _log.LogError($"Error saving. {ex}");
        }
        Hooks.BeforeChatMessageSystemUpdates -= RunBeforeChatMessageSystemUpdates;
        Hooks.AfterChatMessageSystemUpdates -= RunAfterChatMessageSystemUpdates;
        Hooks.BeforeWorldSave -= Save;
        EventRunner = null;
        EventsConfig = null;
    }

    private static void RunBeforeChatMessageSystemUpdates()
    {
        if (!WorldUtil.IsServerInitialized)
        {
            return;
        }
        EventRunner?.OnBeforeChatMessageSystemUpdates();
    }

    private static void RunAfterChatMessageSystemUpdates()
    {
        if (!WorldUtil.IsServerInitialized)
        {
            return;
        }
        EventRunner?.OnAfterChatMessageSystemUpdates();
    }    

}