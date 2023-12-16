using System.Collections.Generic;

namespace EventScheduler.Models;

public class ScheduledEventRaw {
    public string eventId { get; set; }
    public Schedule schedule {get; set; }
    public CommandExecuter executingAdmin { get; set; }
    public List<string> chatCommands { get; set; }


    public class Schedule {
        public string frequency { get; set; }
        public string firstRun { get; set; }
        
    }

    public class CommandExecuter {
        public ulong steamId {get; set; }
    }

}