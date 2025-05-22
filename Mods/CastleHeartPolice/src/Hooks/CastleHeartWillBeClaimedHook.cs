using System.Text;
using CastleHeartPolice.Services;
using HarmonyLib;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.CastleTerritory;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Utilities;

namespace CastleHeartPolice.Hooks;


[HarmonyPatch(typeof(CastleHeartEventSystem), nameof(CastleHeartEventSystem.OnUpdate))]
public static class CastleHeartWillBeClaimedHook {

    public static void Prefix(CastleHeartEventSystem __instance) {
        var entityManager = WorldUtil.Server.EntityManager;
        var jobs = __instance._CastleHeartInteractEventQuery.ToEntityArray(Allocator.Temp);
        foreach (var job in jobs) {
            var heartEvent = entityManager.GetComponentData<CastleHeartInteractEvent>(job);
            if (heartEvent.EventType == CastleHeartInteractEventType.Claim) {
                HandleCastleHeartWillBeClaimed(job, heartEvent);
            }
        }
    }

    private static void HandleCastleHeartWillBeClaimed(Entity job, CastleHeartInteractEvent heartEvent) {
        var entityManager = WorldUtil.Server.EntityManager;
        var castleHeart = CastleHeartUtil.FindCastleHeartById(heartEvent.CastleHeart);        
        var fromCharacter = entityManager.GetComponentData<FromCharacter>(job);
        var user = entityManager.GetComponentData<User>(fromCharacter.User);
        var ruleResult = RulesService.Instance.CheckRuleClaimCastleHeart(fromCharacter.Character, castleHeart);
        if (ruleResult.IsViolation) {
            var message = new StringBuilder("CASTLE CLAIM DENIED!\n");
            foreach (var reason in ruleResult.ViolationReasons) {
                message.AppendLine($"{reason}");
            }
            ChatUtil.SendSystemMessageToClient(user, message.ToString());
            SystemPatchUtil.CancelJob(job);
        }
    }

}
