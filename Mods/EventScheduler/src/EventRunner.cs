using System;
using System.Collections.Generic;
using cheesasaurus.VRisingMods.EventScheduler.Config;
using cheesasaurus.VRisingMods.EventScheduler.Models;
using cheesasaurus.VRisingMods.EventScheduler.Repositories;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.EventScheduler;


public class EventRunner {
    private IEventHistoryRepository EventHistory;
    private EventsConfig EventsConfig;

    private Dictionary<string, DateTime> _nextRunTimes = [];
    private Queue<ScheduledEvent> _testRunQueue = [];

    public EventRunner(EventsConfig eventsConfig, IEventHistoryRepository eventHistory)
    {
        EventsConfig = eventsConfig;
        EventHistory = eventHistory;
    }

    public void OnBeforeChatMessageSystemUpdates()
    {
        while (_testRunQueue.TryDequeue(out var testRunEvent))
        {
            LogUtil.LogMessage($"{DateTime.Now}: Doing a test run of the event {testRunEvent.EventId}");
            try
            {
                SpawnChatCommands(testRunEvent);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }            
        }
        
        foreach (var scheduledEvent in EventsConfig.ScheduledEvents)
        {
            var nextRun = GetOrDetermineNextRun(scheduledEvent);
            if (nextRun <= DateTime.Now)
            {
                var overdueCutoff = nextRun + scheduledEvent.Schedule.OverdueTolerance;
                if (overdueCutoff < DateTime.Now)
                {
                    LogUtil.LogWarning($"{DateTime.Now}: Skipping overdue event {scheduledEvent.EventId}\n  scheduled time : {nextRun}\n  overdue cutoff:{overdueCutoff}");
                }
                else
                {
                    LogUtil.LogMessage($"{DateTime.Now}: Running the event {scheduledEvent.EventId}");
                    EventHistory.SetLastRun(scheduledEvent.EventId, nextRun);
                    try
                    {
                        SpawnChatCommands(scheduledEvent);
                    }
                    catch (Exception ex)
                    {
                        LogUtil.LogError(ex);
                    }
                }
                var nextRunAfterThis = DetermineNextRun(scheduledEvent);
                LogUtil.LogDebug($"Setting next run: {nextRunAfterThis}");
                _nextRunTimes[scheduledEvent.EventId] = nextRunAfterThis;
            }
        }
    }

    public void OnAfterChatMessageSystemUpdates()
    {
        TryRestoreExecutingUserPrivileges();
    }

    public void PrepareTestRun(ScheduledEvent scheduledEvent)
    {
        _testRunQueue.Enqueue(scheduledEvent);
    }

    private void SpawnChatCommands(ScheduledEvent scheduledEvent)
    {
        if (!TryGetOrFindExecutingUser(out UserModel executingUser))
        {
            LogUtil.LogError($"Could not run event {scheduledEvent.EventId}: there is no user with steamId {EventsConfig.ExecuterSteamId}");
            return;
        }
        TryElevateExecutingUserPrivileges();
        foreach (var message in scheduledEvent.ChatCommands)
        {
            ChatUtil.ForgeMessage(executingUser, message);
        }
    }

    private UserModel _executingUser;
    private bool TryGetOrFindExecutingUser(out UserModel userModel)
    {
        if (_executingUser is not null)
        {
            userModel = _executingUser;
            return true;
        }
        
        if (UserUtil.TryFindUserByPlatformId(EventsConfig.ExecuterSteamId, out userModel))
        {
            _executingUser = userModel;
            return true;
        }
        return false;
    }

    private bool _wereUserPrivsModified = false;
    private bool _wasUserAdmin = false;

    private bool TryElevateExecutingUserPrivileges()
    {
        if (_wereUserPrivsModified)
        {
            // already elevated
            return false;
        }
        if (!TryGetOrFindExecutingUser(out var userModel))
        {
            return false;
        }
        _wasUserAdmin = UserUtil.IsAdminForPluginChatCommands(userModel.Entity);
        _wereUserPrivsModified = true;
        UserUtil.HaxSetIsAdminForPluginChatCommands(userModel.Entity, true);
        return true;
    }

    private bool TryRestoreExecutingUserPrivileges()
    {
        if (!_wereUserPrivsModified)
        {
            return false;
        }
        if (!TryGetOrFindExecutingUser(out var userModel))
        {
            return false;
        }
        _wereUserPrivsModified = false;
        UserUtil.HaxSetIsAdminForPluginChatCommands(userModel.Entity, _wasUserAdmin);
        return true;
    }

    private DateTime GetOrDetermineNextRun(ScheduledEvent scheduledEvent)
    {
        DateTime nextRun;
        if (_nextRunTimes.TryGetValue(scheduledEvent.EventId, out nextRun))
        {
            return nextRun;
        }
        nextRun = DetermineNextRun(scheduledEvent);
        _nextRunTimes[scheduledEvent.EventId] = nextRun;
        return nextRun;
    }


    private DateTime DetermineNextRun(ScheduledEvent scheduledEvent)
    {
        var now = DateTime.Now;
        var schedule = scheduledEvent.Schedule;
        var nextRun = schedule.FirstRun;

        // Keep in mind that the schedule might be edited after already running.
        // This is why we check that lastRun >= nextRun
        var ran = EventHistory.TryGetLastRun(scheduledEvent.EventId, out var lastRun);
        if (ran && lastRun >= nextRun)
        {
            nextRun = AddTime(lastRun, schedule.Frequency);
        }

        var cursor = nextRun + scheduledEvent.Schedule.OverdueTolerance;
        while (cursor < now)
        {
            nextRun = AddTime(nextRun, scheduledEvent.Schedule.Frequency);
            cursor = nextRun + scheduledEvent.Schedule.OverdueTolerance;
        }
        return nextRun;
    }

    private static DateTime AddTime(DateTime dt, Frequency frequency)
    {
        switch (frequency.Unit)
        {
            case FrequencyUnit.Week:
                return dt.AddDays(7 * frequency.Value);
            case FrequencyUnit.Day:
                return dt.AddDays(frequency.Value);
            case FrequencyUnit.Hour:
                return dt.AddHours(frequency.Value);
            case FrequencyUnit.Minute:
                return dt.AddMinutes(frequency.Value);
            case FrequencyUnit.Second:
                return dt.AddSeconds(frequency.Value);
            default:
                throw new Exception($"The frequency unit {frequency.Unit} isn't handled");
        }
    }

}