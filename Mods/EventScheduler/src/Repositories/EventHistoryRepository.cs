using System;
using System.Collections.Generic;

namespace EventScheduler.Repositories;

public class EventHistoryRepository {

    private Dictionary<string, DateTime> LastRuns = new(); // todo: this will need to be persisted

    public bool TryGetLastRun(string eventId, out DateTime lastRun) {
        return LastRuns.TryGetValue(eventId, out lastRun);
    }

    public void SetLastRun(string eventId, DateTime lastRun) {
        LastRuns[eventId] = lastRun;
    }

}