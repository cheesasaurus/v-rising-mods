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

    private Dictionary<string, DateTime> _nextRunTimes = new();

    // todo: configurable overdueTolerance. should probably let this vary per-event.
    // a weekly event could make sense to start an hour late. But not e.g. something running every 15 minutes.
    private TimeSpan OverdueTolerance = TimeSpan.FromMilliseconds(2000);

    public EventRunner(EventsConfig eventsConfig, IEventHistoryRepository eventHistory)
    {
        EventsConfig = eventsConfig;
        EventHistory = eventHistory;
    }

    public void Tick() {
        foreach (var scheduledEvent in EventsConfig.ScheduledEvents) {
            var nextRun = GetOrDetermineNextRun(scheduledEvent);
            if (nextRun <= DateTime.Now) {
                var overdueCutoff = nextRun + OverdueTolerance;
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
                        RunChatCommands(scheduledEvent);
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

    private void RunChatCommands(ScheduledEvent scheduledEvent) {
        var userExists = UserUtil.TryFindUserByPlatformId(scheduledEvent.ExecuterSteamId, out var executingUser);
        if (!userExists) {
            LogUtil.LogError($"Could not run event {scheduledEvent.EventId}: there is no user with steamId {scheduledEvent.ExecuterSteamId}");
            return;
        }
        foreach (var message in scheduledEvent.ChatCommands) {
            ChatUtil.ForgeMessage(executingUser, message);
        }
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
        var firstRun = scheduledEvent.Schedule.FirstRun;
        var ran = EventHistory.TryGetLastRun(scheduledEvent.EventId, out var lastRun);

        var nextRun = firstRun;

        // Keep in mind that the schedule might be edited after already running.
        // This is why we check that lastRun >= nextRun
        if (ran && lastRun >= nextRun)
        {
            nextRun = AddTime(lastRun, scheduledEvent.Schedule.Frequency);
        }

        var cursor = nextRun + OverdueTolerance;
        while (cursor < now)
        {
            nextRun = AddTime(nextRun, scheduledEvent.Schedule.Frequency);
            cursor = nextRun + OverdueTolerance;
        }
        return nextRun;
    }

    // todo: use TimeSpan?
    private static DateTime AddTime(DateTime dt, Frequency frequency)
    {
        switch (frequency.Unit)
        {
            case FrequencyUnit.Year:
                return dt.AddYears(frequency.Value);
            case FrequencyUnit.Month:
                return dt.AddMonths(frequency.Value);
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