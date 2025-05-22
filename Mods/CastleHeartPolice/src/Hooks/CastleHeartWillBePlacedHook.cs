using System.Text;
using CastleHeartPolice.Services;
using HarmonyLib;
using ProjectM;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.CastleTerritory;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Prefabs;
using VRisingMods.Core.Utilities;

namespace CastleHeartPolice.Hooks;


[HarmonyPatch(typeof(PlaceTileModelSystem), nameof(PlaceTileModelSystem.OnUpdate))]
public static class CastleHeartWillBePlacedHook {

    public static void Prefix(PlaceTileModelSystem __instance) {
        var jobs = __instance._BuildTileQuery.ToEntityArray(Allocator.Temp);
        foreach (var job in jobs) {
            if (IsCastleHeart(job)) {
                HandleCastleHeartWillBePlaced(job);
            }
        }
    }
    
    private static bool IsCastleHeart(Entity job) {
        var entityManager = WorldUtil.Server.EntityManager;
        var buildTileModelData = entityManager.GetComponentData<BuildTileModelEvent>(job);
        return buildTileModelData.PrefabGuid.Equals(TileModelPrefabs.TM_BloodFountain_Pylon_Station);
    }

    private static void HandleCastleHeartWillBePlaced(Entity job) {
        var entityManager = WorldUtil.Server.EntityManager;
        var buildTileModelData = entityManager.GetComponentData<BuildTileModelEvent>(job);
        var worldPos = buildTileModelData.SpawnTranslation.Value;
        var foundTerritory = CastleTerritoryUtil.TryFindTerritoryContaining(worldPos, out var territoryInfo);
        if (!foundTerritory) {
            return;
        }
        var fromCharacter = entityManager.GetComponentData<FromCharacter>(job);
        var user = entityManager.GetComponentData<User>(fromCharacter.User);
        var ruleResult = RulesService.Instance.CheckRulePlaceCastleHeartInTerritory(fromCharacter.Character, territoryInfo);
        if (ruleResult.IsViolation) {
            var message = new StringBuilder("CASTLE HEART PLACEMENT DENIED!\n");
            foreach (var reason in ruleResult.ViolationReasons) {
                message.AppendLine($"{reason}");
            }
            ChatUtil.SendSystemMessageToClient(user, message.ToString());
            SystemPatchUtil.CancelJob(job);
        }
    }

}
