using System;

namespace cheesasaurus.VRisingMods.EventScheduler.Repositories;

public interface IEventHistoryRepository
{
    public bool TryGetLastRun(string eventId, out DateTime lastRun);
    public void SetLastRun(string eventId, DateTime lastRun);
    public bool TryLoad();
    public bool TrySave();
}
