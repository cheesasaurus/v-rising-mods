using VampireCommandFramework;
using VRisingMods.Core.Buff;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace ShardPolice.Commands;


public class ShardBuffsRemoveEveryoneCommand {

    [Command("shard-buffs-remove-everyone", shortHand: "sbre", description: "remove shard buffs from everyone", adminOnly: true)]
    public void Execute(ChatCommandContext ctx) {
        foreach (var user in UserUtil.FindAllUsers()) {
            var wasABuffRemoved = ShardRelicBuffUtil.TryRemoveShardBuffsFromPlayer(user.User.LocalCharacter._Entity);
            if (wasABuffRemoved) {
                ChatUtil.SendSystemMessageToClient(user.User, $"Your shard buffs were removed by an admin ({ctx.User.CharacterName})");
            }
            
        }
        LogUtil.LogInfo($"Shard buffs were removed from all players by {ctx.User.CharacterName} (steam#{ctx.User.PlatformId}).");
        ctx.Reply("Removed shard buffs from all players.");
    }

}
