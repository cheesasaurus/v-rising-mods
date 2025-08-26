using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using BepInEx;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.EventScheduler.Repositories;

public class EventHistoryRepository_JSON : IEventHistoryRepository
{
    private string FilePath;

    private Dictionary<string, DateTimeOffset> _lastRunTime_byEventId = new();

    public EventHistoryRepository_JSON(string pluginGUID, string filename)
    {
        var bepinexPath = Path.GetFullPath(Path.Combine(Paths.ConfigPath, @"..\"));
        var dir = Path.Combine(bepinexPath, @"PluginSaveData", pluginGUID);
        Directory.CreateDirectory(dir);
        FilePath = Path.Combine(dir, filename);
    }

    public bool TryGetLastRun(string eventId, out DateTimeOffset lastRun)
    {
        return _lastRunTime_byEventId.TryGetValue(eventId, out lastRun);
    }

    public void SetLastRun(string eventId, DateTimeOffset lastRun)
    {
        _lastRunTime_byEventId[eventId] = lastRun;
    }

    public bool TryLoad()
    {
        if (!File.Exists(FilePath))
        {
            LogUtil.LogDebug($"Cannot load EventHistory from non-existent file: {FilePath}");
            return false;
        }
        
        try
        {
            var json = File.ReadAllText(FilePath);
            _lastRunTime_byEventId = JsonSerializer.Deserialize<Dictionary<string, DateTimeOffset>>(json);
            return true;
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Could not load EventHistory: {ex}");
            return false;
        }
    }

    public bool TrySave()
    {
        try
        {
            var json = JsonSerializer.Serialize(_lastRunTime_byEventId);
            File.WriteAllText(FilePath, json);
            return true;
        }
        catch (Exception ex)
        {
            LogUtil.LogError($"Could not save EventHistory: {ex}");
            return false;
        }
    }

}