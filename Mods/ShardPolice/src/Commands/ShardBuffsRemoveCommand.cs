using Bloodstone.API;
using ProjectM;
using VampireCommandFramework;
using VRisingMods.Core.Buff;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace ShardPolice.Commands;


public class ShardBuffsRemoveCommand {

    [Command("shard-buffs-remove", shortHand: "sbr", description: "remove shard buffs from a player", adminOnly: true)]
    public void Execute(ChatCommandContext ctx, string playerName = "") {
        if (playerName.Equals("")) {
            ShardRelicBuffUtil.TryRemoveShardBuffsFromPlayer(ctx.User.LocalCharacter._Entity);
            ctx.Reply("Removed all shard buffs from self.");
            return;
        }
        
        if (!UserUtil.TryFindUserByName(playerName, out var user)) {
            ctx.Reply($"No player found named \"{playerName}\"");
            return;
        }

        var targetUser = user.User;
        var properlyCasedName = targetUser.CharacterName;
        var wasABuffRemoved = ShardRelicBuffUtil.TryRemoveShardBuffsFromPlayer(targetUser.LocalCharacter._Entity);
        if (!wasABuffRemoved) {
            ctx.Reply($"{properlyCasedName} did not have any shard buffs to remove.");
            return;
        }

        ChatUtil.SendSystemMessageToClient(targetUser, $"Your shard buffs were removed by an admin ({ctx.User.CharacterName})");
        LogUtil.LogInfo($"Shard buffs were removed from player {properlyCasedName} (steam#{targetUser.PlatformId}) by admin {ctx.User.CharacterName} (steam#{ctx.User.PlatformId}).");
        ctx.Reply($"Removed shard buffs from {properlyCasedName}.");
    }

}
