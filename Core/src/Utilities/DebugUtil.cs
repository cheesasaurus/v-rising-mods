using Unity.Entities;

namespace VRisingMods.Core.Utilities;

public static class DebugUtil {

    public static void LogComponentTypes(Entity entity) {
        LogUtil.LogMessage("-------------------------------------------");
        var componentTypes = WorldUtil.Server.EntityManager.GetComponentTypes(entity);
        foreach (var componentType in componentTypes) {
            LogUtil.LogMessage(componentType.ToString());
        }
        LogUtil.LogMessage("-------------------------------------------");
    }

}
