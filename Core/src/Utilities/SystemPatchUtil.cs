using ProjectM;
using ProjectM.Shared;
using Unity.Entities;

namespace VRisingMods.Core.Utilities;

public static class SystemPatchUtil {
    public static void CancelJob(Entity entity) {
        WorldUtil.Server.EntityManager.AddComponent<Disabled>(entity);
        DestroyUtility.CreateDestroyEvent(WorldUtil.Server.EntityManager, entity, DestroyReason.Default, DestroyDebugReason.ByScript);
    }
}