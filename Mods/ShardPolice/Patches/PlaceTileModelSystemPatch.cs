using HarmonyLib;
using ProjectM;
using ProjectM.Shared;
using ShardPolice.Utils;
using Unity.Entities;

namespace ShardPolice.Patches;

public static class PlaceTileModelSystemPatches {

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyCanDismantle))]
    public static class VerifyCanDismantle {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity) {
            if (entityManager.HasComponent<Relic>(tileModelEntity)) {
                if (ShardPoliceConfig.PreventShardOwnersMovingPlacedShardDuringRaidHours.Value && RaidTimeUtil.IsRaidTimeNow()) {
                    __result = false;
                }
            }
        }
    }

    [HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.VerifyIfCanMoveOrRotateAfterBuilt))]
    public static class VerifyIfCanMoveOrRotateAfterBuilt {
        public static void Postfix(ref bool __result, EntityManager entityManager, Entity tileModelEntity) {
            if (entityManager.HasComponent<Relic>(tileModelEntity)) {
                if (ShardPoliceConfig.PreventShardOwnersMovingPlacedShardDuringRaidHours.Value && RaidTimeUtil.IsRaidTimeNow()) {
                    __result = false;
                }
            }
        }
    }

}

