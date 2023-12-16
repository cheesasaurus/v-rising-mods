using BepInEx.Logging;

namespace VRisingMods.Core.Utilities;

public static class LogUtil {
    public static ManualLogSource Logger;

    public static void Init(ManualLogSource logger) {
        Logger = logger;
    }

    public static void LogMessage(string str) {
        Logger.LogMessage(str);
    }
}