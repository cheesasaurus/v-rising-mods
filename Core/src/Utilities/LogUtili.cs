using BepInEx.Logging;

namespace VRisingMods.Core.Utilities;

public static class LogUtil
{
    public static ManualLogSource Logger;

    public static void Init(ManualLogSource logger)
    {
        Logger = logger;
    }

    public static void LogMessage(object data)
    {
        Logger.LogMessage(data);
    }

    public static void LogInfo(object data)
    {
        Logger.LogInfo(data);
    }

    public static void LogError(object data)
    {
        Logger.LogError(data);
    }
    
    public static void LogWarning(object data) {
        Logger.LogWarning(data);
    }
    
}