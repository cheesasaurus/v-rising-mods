using System.Collections.Generic;

namespace cheesasaurus.VRisingMods.EventScheduler.Models;

class EventsConfigRaw
{
    public CommandExecuter executingAdmin { get; set; }
    public List<ScheduledEventRaw> events { get; set; }

    public class CommandExecuter
    {
        public ulong steamId { get; set; }
    }
    
}