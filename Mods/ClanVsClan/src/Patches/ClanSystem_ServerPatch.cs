using System.Text;
using Bloodstone.API;
using ClanVsClan.Utils;
using HarmonyLib;
using ProjectM;
using ProjectM.Gameplay.Clan;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ClanVsClan.Patches;


[HarmonyPatch(typeof(ClanSystem_Server), nameof(ClanSystem_Server.OnUpdate))]
public static class ClanSystem_ServerPatch
{
    public static void Prefix(ClanSystem_Server __instance)
    {
        var inviteRequestJobs = __instance._InvitePlayerToClanQuery.ToEntityArray(Allocator.Temp);
        foreach (var job in inviteRequestJobs) {
            HandleInviteRequest(job);
        }

        var inviteResponseJobs = __instance._ClanInviteResponseQuery.ToEntityArray(Allocator.Temp);
        foreach (var job in inviteResponseJobs) {
            HandleInviteResponse(job);
        }

        var createClanJobs = __instance._CreateClanEventQuery.ToEntityArray(Allocator.Temp);
        foreach (var job in createClanJobs) {
            HandleClanWillBeCreated(job);
        }
    }

    private static void HandleInviteRequest(Entity job) {
        if (RaidTimeUtil.IsRaidTimeNow()) {
            CancelClanJobWithMessage(job, "CANNOT INVITE DURING RAID TIME!");
        }
    }

    private static void HandleInviteResponse(Entity job) {
        var entityManager = VWorld.Server.EntityManager;
        var inviteResponse = entityManager.GetComponentData<ClanEvents_Client.ClanInviteResponse>(job);
        if (!inviteResponse.Response.Equals(InviteRequestResponse.Accept)) {
            return;
        }
        
        if (RaidTimeUtil.IsRaidTimeNow()) {
            CancelClanJobWithMessage(job, "CANNOT JOIN CLAN DURING RAID TIME!");
        }
    }

    private static void HandleClanWillBeCreated(Entity job) {
        if (RaidTimeUtil.IsRaidTimeNow()) {
            CancelClanJobWithMessage(job, "CANNOT CREATE CLAN DURING RAID TIME!");
        }
    }

    private static void CancelClanJobWithMessage(Entity job, string message) {
        var entityManager = VWorld.Server.EntityManager;
        var fromCharacter = entityManager.GetComponentData<FromCharacter>(job);
        var user = entityManager.GetComponentData<User>(fromCharacter.User);
        ServerChatUtils.SendSystemMessageToClient(entityManager, user, message);
        SystemPatchUtil.CancelJob(job);
    }

}
