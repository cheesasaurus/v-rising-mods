using System;
using System.Collections.Generic;
using BepInEx.Logging;
using cheesasaurus.VRisingMods.EventScheduler.Config;
using cheesasaurus.VRisingMods.EventScheduler.Models;
using cheesasaurus.VRisingMods.EventScheduler.Repositories;
using ProjectM;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.EventScheduler;


public class EventRunner {
    private readonly ManualLogSource _log;
    private readonly IEventHistoryRepository _eventHistory;
    private readonly EventsConfig _eventsConfig;

    private readonly Dictionary<string, DateTimeOffset> _nextRunTimes = [];
    private readonly Queue<ScheduledEvent> _testRunQueue = [];

    private readonly ChatMessageSystem _chatMessageSystem;
    private bool _wasChatMessageSystemEnabled = false;

    public EventRunner(ManualLogSource log, EventsConfig eventsConfig, IEventHistoryRepository eventHistory)
    {
        _log = log;
        _eventsConfig = eventsConfig;
        _eventHistory = eventHistory;
        _chatMessageSystem = WorldUtil.Server.GetExistingSystemManaged<ChatMessageSystem>();
    }

    public void OnBeforeChatMessageSystemUpdates()
    {
        _wasChatMessageSystemEnabled = _chatMessageSystem.Enabled;
        bool didSpawnChatCommands = false;
        var now = DateTimeOffset.Now;

        while (_testRunQueue.TryDequeue(out var testRunEvent))
        {
            _log.LogMessage($"{now}: Doing a test run of the event {testRunEvent.EventId}");
            try
            {
                didSpawnChatCommands |= TrySpawnChatCommands(testRunEvent);
            }
            catch (Exception ex)
            {
                _log.LogError(ex);
            }
        }

        foreach (var scheduledEvent in _eventsConfig.ScheduledEvents)
        {
            var nextRun = GetOrDetermineNextRun(scheduledEvent);
            if (nextRun <= now)
            {
                var overdueCutoff = nextRun + scheduledEvent.Schedule.OverdueTolerance;
                if (overdueCutoff < now)
                {
                    _log.LogWarning($"{now}: Skipping overdue event {scheduledEvent.EventId}\n  scheduled time : {nextRun}\n  overdue cutoff:{overdueCutoff}");
                }
                else
                {
                    _log.LogMessage($"{now}: Running the event {scheduledEvent.EventId}");
                    _eventHistory.SetLastRun(scheduledEvent.EventId, nextRun);
                    try
                    {
                        didSpawnChatCommands |= TrySpawnChatCommands(scheduledEvent);
                    }
                    catch (Exception ex)
                    {
                        _log.LogError(ex);
                    }
                }
                var nextRunAfterThis = DetermineNextRun(scheduledEvent);
                _log.LogDebug($"Setting next run: {nextRunAfterThis}");
                _nextRunTimes[scheduledEvent.EventId] = nextRunAfterThis;
            }
        }

        if (didSpawnChatCommands)
        {
            _chatMessageSystem.Enabled = true;
            TryElevateExecutingUserPrivileges();
        }
    }

    public void OnAfterChatMessageSystemUpdates()
    {
        _chatMessageSystem.Enabled = _wasChatMessageSystemEnabled;
        TryRestoreExecutingUserPrivileges();
    }

    public void PrepareTestRun(ScheduledEvent scheduledEvent)
    {
        _testRunQueue.Enqueue(scheduledEvent);
    }

    private bool TrySpawnChatCommands(ScheduledEvent scheduledEvent)
    {
        if (!TryGetOrFindExecutingUser(out UserModel executingUser))
        {
            _log.LogError($"Could not run event {scheduledEvent.EventId}: there is no user with steamId {_eventsConfig.ExecuterSteamId}");
            return false;
        }
        foreach (var message in scheduledEvent.ChatCommands)
        {
            ChatUtil.ForgeMessage(executingUser, message);
        }
        return true;
    }

    private UserModel _executingUser;
    private bool TryGetOrFindExecutingUser(out UserModel userModel)
    {
        if (_executingUser is not null)
        {
            userModel = _executingUser;
            return true;
        }
        
        if (UserUtil.TryFindUserByPlatformId(_eventsConfig.ExecuterSteamId, out userModel))
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

    private DateTimeOffset GetOrDetermineNextRun(ScheduledEvent scheduledEvent)
    {
        DateTimeOffset nextRun;
        if (_nextRunTimes.TryGetValue(scheduledEvent.EventId, out nextRun))
        {
            return nextRun;
        }
        nextRun = DetermineNextRun(scheduledEvent);
        _nextRunTimes[scheduledEvent.EventId] = nextRun;
        return nextRun;
    }


    private DateTimeOffset DetermineNextRun(ScheduledEvent scheduledEvent)
    {
        var now = DateTimeOffset.Now;
        var schedule = scheduledEvent.Schedule;
        var nextRun = schedule.FirstRun;

        // Keep in mind that the schedule might be edited after already running.
        // In that case, lastRun could be less than firstRun.
        var ran = _eventHistory.TryGetLastRun(scheduledEvent.EventId, out var lastRun);
        bool ignoreLastRun = !ran || lastRun < schedule.FirstRun;

        if (!ignoreLastRun)
        {
            nextRun = AddTime(lastRun, schedule.Frequency);
        }

        bool alreadyRan(DateTimeOffset candidateRun) => !ignoreLastRun && candidateRun <= lastRun;
        var cursor = nextRun + scheduledEvent.Schedule.OverdueTolerance;
        while (cursor < now || alreadyRan(nextRun))
        {
            nextRun = AddTime(nextRun, scheduledEvent.Schedule.Frequency);
            cursor = nextRun + scheduledEvent.Schedule.OverdueTolerance;
        }
        return nextRun;
    }

    private static DateTimeOffset AddTime(DateTimeOffset dt, Frequency frequency)
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