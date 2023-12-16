using System.Text;
using Bloodstone.API;
using CastleHeartPolice.Services;
using CastleHeartPolice.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Clan;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace CastleHeartPolice.Hooks;


[HarmonyPatch(typeof(ClanSystem_Server), nameof(ClanSystem_Server.OnUpdate))]
public static class PlayerAcceptedClanInviteHook
{
    public static void Prefix(ClanSystem_Server __instance)
    {
        var inviteResponseThings = __instance._ClanInviteResponseQuery.ToEntityArray(Allocator.Temp);
        foreach (var job in inviteResponseThings) {
            HandleInviteResponse(job);
        }
    }

    private static void HandleInviteResponse(Entity job) {
        var entityManager = VWorld.Server.EntityManager;
        var inviteResponse = entityManager.GetComponentData<ClanEvents_Client.ClanInviteResponse>(job);
        if (!inviteResponse.Response.Equals(InviteRequestResponse.Accept)) {
            return;
        }
        var fromCharacter = entityManager.GetComponentData<FromCharacter>(job);
        var user = entityManager.GetComponentData<User>(fromCharacter.User);

        var foundClan = ClanUtil.TryFindClan(inviteResponse.ClanId, out var clan);
        if (!foundClan) {
            return;
        }

        var ruleResult = RulesService.Instance.CheckRuleJoinClan(fromCharacter.Character, clan);
        if (ruleResult.IsViolation) {
            var message = new StringBuilder("CLAN JOINING DENIED!\n");
            foreach (var reason in ruleResult.ViolationReasons) {
                message.AppendLine($"{reason}");
            }
            ServerChatUtils.SendSystemMessageToClient(entityManager, user, message.ToString());
            SystemPatchUtil.CancelJob(job);
        }
    }

}
