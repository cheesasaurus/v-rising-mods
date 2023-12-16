using Bloodstone.API;
using ProjectM;
using ProjectM.Shared;
using Unity.Entities;

namespace CastleHeartPolice.Utils;

public static class SystemPatchUtil {
    public static void CancelJob(Entity entity) {
        VWorld.Server.EntityManager.AddComponent<Disabled>(entity);
        DestroyUtility.CreateDestroyEvent(VWorld.Server.EntityManager, entity, DestroyReason.Default, DestroyDebugReason.ByScript);
    }
}