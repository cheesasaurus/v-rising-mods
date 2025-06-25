using System.Collections.Generic;

namespace cheesasaurus.VRisingMods.EventScheduler.Models;

public class ScheduledEventRaw {
    public string eventId { get; set; }
    public Schedule schedule {get; set; }
    public List<string> chatCommands { get; set; }


    public class Schedule {
        public string frequency { get; set; }
        public string firstRun { get; set; }
    }

}