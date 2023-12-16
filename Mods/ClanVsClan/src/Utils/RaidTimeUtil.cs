using System;
using System.Collections.Generic;
using Bloodstone.API;
using ProjectM;

namespace ClanVsClan.Utils;

public static class RaidTimeUtil {
    private static List<DayOfWeek> WeekendDays = [DayOfWeek.Saturday, DayOfWeek.Sunday];
    private static ServerGameSettingsSystem serverGameSettingsSystem = VWorld.Server.GetExistingSystem<ServerGameSettingsSystem>();

    public static bool IsRaidTimeNow() {
        return IsRaidTime(DateTime.Now);
    }

    public static bool IsRaidTime(DateTime dt) {
        switch (serverGameSettingsSystem._Settings.CastleDamageMode) {
            default:
                return false;
            case CastleDamageMode.Never:
                return false;
            case CastleDamageMode.Always:
                return true;
            case CastleDamageMode.TimeRestricted:
                var pis = serverGameSettingsSystem._Settings.PlayerInteractionSettings;
                // might need some funny business with <pis.TimeZone>,
                // but it seems like the game only ever uses local time, regardless of that config?
                var window = IsWeekend(dt) ? pis.VSCastleWeekendTime : pis.VSCastleWeekdayTime;
                return IsInWindow(dt, window);
        }
    }

    private static bool IsWeekend(DateTime dt) {
        return WeekendDays.Contains(dt.DayOfWeek);
    }

    private static bool IsInWindow(DateTime dt, StartEndTimeData window) {
        return dt.Hour >= window.StartHour
            && dt.Minute >= window.StartMinute
            && dt.Hour < window.EndHour
            && dt.Minute < window.EndMinute;
    }

}
