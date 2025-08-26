using System;

namespace cheesasaurus.VRisingMods.EventScheduler.Repositories;

public interface IEventHistoryRepository
{
    public bool TryGetLastRun(string eventId, out DateTimeOffset lastRun);
    public void SetLastRun(string eventId, DateTimeOffset lastRun);
    public bool TryLoad();
    public bool TrySave();
}
