using System.Text;
using Bloodstone.API;
using CastleHeartPolice.Prefabs;
using CastleHeartPolice.Services;
using CastleHeartPolice.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Systems;
using ProjectM.Network;
using ProjectM.UI.CastleHeartUI;
using Unity.Collections;
using Unity.Entities;

namespace CastleHeartPolice.Hooks;


[HarmonyPatch(typeof(PylonstationSystem), nameof(PylonstationSystem.OnUpdate))]
public static class CastleHeartWillBeClaimedHook {
    
    public static void Prefix(PylonstationSystem __instance) {
        var jobs = __instance._ClaimPylonQuery.ToEntityArray(Allocator.Temp);
        foreach (var job in jobs) {
            HandleCastleHeartWillBeClaimed(job);
        }
    }

    private static void HandleCastleHeartWillBeClaimed(Entity job) {
        var entityManager = VWorld.Server.EntityManager;

        var claimPylonEvent = entityManager.GetComponentData<ClaimPylonEvent>(job);
        if (claimPylonEvent.IsConsoleCommand) {
            return;
        }
        var castleHeart = CastleHeartUtil.FindCastleHeartById(claimPylonEvent.Workstation);
        
        var fromCharacter = entityManager.GetComponentData<FromCharacter>(job);
        var user = entityManager.GetComponentData<User>(fromCharacter.User);
        var ruleResult = RulesService.Instance.CheckRuleClaimCastleHeart(fromCharacter.Character, castleHeart);
        if (ruleResult.IsViolation) {
            var message = new StringBuilder("CASTLE CLAIM DENIED!\n");
            foreach (var reason in ruleResult.ViolationReasons) {
                message.AppendLine($"{reason}");
            }
            ServerChatUtils.SendSystemMessageToClient(entityManager, user, message.ToString());
            SystemPatchUtil.CancelJob(job);
        }
    }

}
