using System;
using EventScheduler.Config;
using EventScheduler.Models;
using EventScheduler.Repositories;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace EventScheduler;


public class EventRunner {
    private EventHistoryRepository EventHistory;
    private EventsConfig EventsConfig;
    public EventRunner(EventsConfig eventsConfig, EventHistoryRepository eventHistory) {
        EventsConfig = eventsConfig;
        EventHistory = eventHistory;
    }

    public void Tick() {
        foreach (var scheduledEvent in EventsConfig.ScheduledEvents) {
            var nextRun = NextRun(scheduledEvent);
            if (nextRun <= DateTime.Now) {
                LogUtil.LogMessage($"{DateTime.Now}: Running the event {scheduledEvent.EventId}");
                EventHistory.SetLastRun(scheduledEvent.EventId, nextRun);
                RunChatCommands(scheduledEvent);
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

    private DateTime NextRun(ScheduledEvent scheduledEvent) {
        var now = DateTime.Now;
        var firstRun = scheduledEvent.Schedule.FirstRun;
        var ran = EventHistory.TryGetLastRun(scheduledEvent.EventId, out var lastRun);
        if (!ran || firstRun > now) {
            return firstRun;
        }

        // keep in mind that the schedule might be edited after already running
        if (lastRun < firstRun) {
            return firstRun;
        }

        // move the cursor forwards at least once, and continue skipping extraneous runs if we're overdue
        var nextRun = lastRun;
        while (true) {
            var nextDueRunPending = AddTime(nextRun, scheduledEvent.Schedule.Frequency);
            if (nextDueRunPending > now && nextRun != lastRun) {
                break;
            }
            nextRun = nextDueRunPending;
        }
        return nextRun;
    }

    private static DateTime AddTime(DateTime dt, Frequency frequency) {
        switch (frequency.Unit) {
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