using System;
using System.IO;
using BepInEx;
using LiteDB;

namespace EventScheduler.Repositories;

public class EventHistoryRepository {
    private string DbPath;

    public EventHistoryRepository(string pluginGUID, string filename) {
        var bepinexPath = Path.GetFullPath(Path.Combine(Paths.ConfigPath, @"..\"));
        var dir = Path.Combine(bepinexPath, @"PluginSaveData", pluginGUID);
        Directory.CreateDirectory(dir);
        DbPath = Path.Combine(dir, filename);

        using (var db = new LiteDatabase(DbPath)) {
            var collection = db.GetCollection<EventEntry>("events");
        }
    }

    public bool TryGetLastRun(string eventId, out DateTime lastRun) {
        using (var db = new LiteDatabase(DbPath)) {
            var collection = db.GetCollection<EventEntry>("events");
            var entry = collection.FindById(eventId);
            if (entry is not null) {
                lastRun = entry.LastRun;
                return true;
            }
        }
        lastRun = default;
        return false;
    }

    public void SetLastRun(string eventId, DateTime lastRun) {
        using (var db = new LiteDatabase(DbPath)) {
            var collection = db.GetCollection<EventEntry>("events");
            collection.Upsert(new EventEntry() {
                _id = eventId,
                LastRun = lastRun
            });
        }
    }

    private class EventEntry {
        public string _id { get; set; }
        public DateTime LastRun { get; set; }
    }

}