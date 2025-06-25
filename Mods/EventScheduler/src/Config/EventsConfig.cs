using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using cheesasaurus.VRisingMods.EventScheduler.Models;
using VRisingMods.Core.Config;

namespace cheesasaurus.VRisingMods.EventScheduler.Config;


public class EventsConfig : AbstractJsonConfig
{
    public ulong ExecuterSteamId;
    public List<ScheduledEvent> ScheduledEvents = new();

    public EventsConfig(string filepath) : base(filepath) {

    }

    public override string ToJson()
    {
        var options = new JsonSerializerOptions {
            WriteIndented = true,
        };

        var eventsRaw = new List<ScheduledEventRaw>();
        foreach (var scheduledEvent in ScheduledEvents) {
            eventsRaw.Add(scheduledEvent._raw);
        }

        var configDTO = new EventsConfigRaw
        {
            executingAdmin = new EventsConfigRaw.CommandExecuter
            {
                steamId = ExecuterSteamId,
            },
            events = eventsRaw,
        };
        return JsonSerializer.Serialize(configDTO, options);
    }

    protected override void InitDefaults() {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "cheesasaurus.VRisingMods.EventScheduler.resources.example-events.jsonc";

        using (Stream stream = assembly.GetManifestResourceStream(resourceName))
        using (StreamReader reader = new StreamReader(stream)) {
            string exampleJson = reader.ReadToEnd();
            InitFromJson(exampleJson);
        }
    }

    protected override void InitFromJson(string json)
    {
        var options = new JsonSerializerOptions {
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        var configRaw = JsonSerializer.Deserialize<EventsConfigRaw>(json, options);
        ExecuterSteamId = configRaw.executingAdmin.steamId;

        ScheduledEvents.Clear();
        foreach (var eventRaw in configRaw.events)
        {
            ScheduledEvents.Add(new ScheduledEvent(eventRaw));
        }
    }

    public static EventsConfig Init(string pluginGUID, string filename) {
        return Init<EventsConfig>(pluginGUID, filename);
    }

}
