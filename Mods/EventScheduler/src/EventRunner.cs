using System;
using System.Collections.Generic;
using EventScheduler.Config;
using EventScheduler.Models;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Utilities;

namespace EventScheduler;


public class EventRunner {
    private Dictionary<string, DateTime> LastRuns = new(); // todo: this will need to be persisted
    private EventsConfig EventsConfig;
    public EventRunner(EventsConfig eventsConfig) {
        EventsConfig = eventsConfig;
    }

    public void Tick() {
        foreach (var scheduledEvent in EventsConfig.ScheduledEvents) {
            var nextRun = NextRun(scheduledEvent);
            if (nextRun <= DateTime.Now) {
                LogUtil.LogMessage($"{DateTime.Now}: Running the event {scheduledEvent.EventId}");
                LastRuns[scheduledEvent.EventId] = nextRun;
                RunChatCommands(scheduledEvent);
            }
        }
    }

    private void RunChatCommands(ScheduledEvent scheduledEvent) {
        foreach (var message in scheduledEvent.ChatCommands) {
            // todo: actual configured steamId
            ChatUtil.ForgeMessage("Dingus", message);
        }
    }

    private DateTime NextRun(ScheduledEvent scheduledEvent) {
        var now = DateTime.Now;
        var firstRun = scheduledEvent.Schedule.FirstRun;
        // keep in mind that the schedule might be edited after already running
        var ran = LastRuns.TryGetValue(scheduledEvent.EventId, out var lastRun);
        if (!ran || firstRun > now) {
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