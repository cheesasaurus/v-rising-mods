using Bloodstone.API;
using ProjectM;
using ShardPolice.Utils;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace ShardPolice.Commands;


public class ShardBuffsRemoveCommand {

    [Command("shard-buffs-remove", shortHand: "sbr", description: "remove shard buffs from a player", adminOnly: true)]
    public void Execute(ChatCommandContext ctx, string playerName = "") {
        if (playerName.Equals("")) {
            ShardBuffUtil.TryRemoveShardBuffsFromPlayer(ctx.User.LocalCharacter._Entity);
            ctx.Reply("Removed all shard buffs from self.");
            return;
        }
        
        if (!UserUtil.TryFindUserByName(playerName, out var nullableUser)) {
            ctx.Reply($"No player found named \"{playerName}\"");
            return;
        }

        var targetUser = nullableUser.Value;
        var properlyCasedName = targetUser.CharacterName;
        var wasABuffRemoved = ShardBuffUtil.TryRemoveShardBuffsFromPlayer(targetUser.LocalCharacter._Entity);
        if (!wasABuffRemoved) {
            ctx.Reply($"{properlyCasedName} did not have any shard buffs to remove.");
            return;
        }

        ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, targetUser, $"Your shard buffs were removed by an admin ({ctx.User.CharacterName})");
        LogUtil.LogInfo($"Shard buffs were removed from player {properlyCasedName} (steam#{targetUser.PlatformId}) by admin {ctx.User.CharacterName} (steam#{ctx.User.PlatformId}).");
        ctx.Reply($"Removed shard buffs from {properlyCasedName}.");
    }

}
