using System.Collections.Generic;
using System.Text.Json;
using EventScheduler.Models;
using VRisingMods.Core.Config;

namespace EventScheduler.Config;


public class EventsConfig : AbstractJsonConfig {

    public List<ScheduledEvent> ScheduledEvents;

    public EventsConfig(string filepath) : base(filepath) {

    }

    public override string ToJson() {
        var options = new JsonSerializerOptions {
            WriteIndented = true,
        };
        var eventsRaw = new List<ScheduledEventRaw>();
        foreach (var scheduledEvent in ScheduledEvents) {
            eventsRaw.Add(scheduledEvent._raw);
        }
        return JsonSerializer.Serialize(eventsRaw, options);
    }

    protected override void InitDefaults() {
        ScheduledEvents = new();
    }

    protected override void InitFromJson(string json) {
        var options = new JsonSerializerOptions {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };
        ScheduledEvents = new();
        var eventsRaw = JsonSerializer.Deserialize<List<ScheduledEventRaw>>(json, options);
        foreach (var raw in eventsRaw) {
            ScheduledEvents.Add(new ScheduledEvent(raw));
        }
    }

    public static EventsConfig Init(string pluginGUID, string filename) {
        return Init<EventsConfig>(pluginGUID, filename);
    }

}
