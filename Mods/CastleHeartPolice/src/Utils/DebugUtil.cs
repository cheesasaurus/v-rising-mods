using Bloodstone.API;
using Unity.Entities;

namespace CastleHeartPolice.Utils;

public static class DebugUtil {

    public static void LogComponentTypes(Entity entity) {
        Plugin.Logger.LogMessage("-------------------------------------------");
        var componentTypes = VWorld.Server.EntityManager.GetComponentTypes(entity);
        foreach (var componentType in componentTypes) {
            Plugin.Logger.LogMessage(componentType.ToString());
        }
        Plugin.Logger.LogMessage("-------------------------------------------");
    }

}
