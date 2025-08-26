using System;
using System.Collections.Generic;

namespace cheesasaurus.VRisingMods.EventScheduler.Models;


public class ScheduledEvent {
    public readonly ScheduledEventRaw _raw;
    public readonly string EventId;
    public readonly List<string> ChatCommands;
    public readonly Schedule Schedule;

    public ScheduledEvent(ScheduledEventRaw raw)
    {
        _raw = raw;
        EventId = raw.eventId;
        ChatCommands = raw.chatCommands;
        Schedule = new Schedule()
        {
            Frequency = new Frequency(raw.schedule.frequency),
            FirstRun = DateTimeOffset.Parse(raw.schedule.firstRun),
            OverdueTolerance = Frequency.AsTimeSpan(raw.schedule.overdueTolerance),
        };
    }

}

public class Schedule
{
    public Frequency Frequency;
    public DateTimeOffset FirstRun;
    public TimeSpan OverdueTolerance;
}

public class Frequency
{
    public readonly int Value;
    public readonly FrequencyUnit Unit;

    public Frequency(string str)
    {
        string[] parts = str.Split(" ");
        if (parts.Length != 2)
        {
            throw new FormatException($"could not parse frequency \"{str}\"");
        }
        Value = int.Parse(parts[0]);
        Unit = UnitFromString(parts[1]);
    }

    public static FrequencyUnit UnitFromString(string str)
    {
        switch (str.ToLower())
        {
            case "w":
            case "week":
            case "weeks":
                return FrequencyUnit.Week;

            case "d":
            case "day":
            case "days":
                return FrequencyUnit.Day;

            case "h":
            case "hour":
            case "hours":
                return FrequencyUnit.Hour;

            case "m":
            case "minute":
            case "minutes":
                return FrequencyUnit.Minute;

            case "s":
            case "second":
            case "seconds":
                return FrequencyUnit.Second;

            default:
                throw new FormatException($"could not parse frequency unit \"{str}\"");
        }
    }

    public TimeSpan ToTimeSpan()
    {
        switch (Unit)
        {
            case FrequencyUnit.Week:
                return TimeSpan.FromDays(Value * 7);
            case FrequencyUnit.Day:
                return TimeSpan.FromDays(Value);
            case FrequencyUnit.Hour:
                return TimeSpan.FromHours(Value);
            case FrequencyUnit.Minute:
                return TimeSpan.FromMinutes(Value);
            case FrequencyUnit.Second:
                return TimeSpan.FromSeconds(Value);
            default:
                return TimeSpan.Zero;
        }
    }

    public static TimeSpan AsTimeSpan(string str)
    {
        var frequency = new Frequency(str);
        return frequency.ToTimeSpan();
    }

}




