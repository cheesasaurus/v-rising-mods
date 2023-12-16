using Bloodstone.API;
using ProjectM;
using ShardPolice.Utils;
using VampireCommandFramework;

namespace ShardPolice.Commands;


public class ShardBuffsRemoveEveryoneCommand {

    [Command("shard-buffs-remove-everyone", shortHand: "sbre", description: "remove shard buffs from everyone", adminOnly: true)]
    public void Execute(ChatCommandContext ctx) {
        foreach (var user in UserUtil.FindAllUsers()) {
            var wasABuffRemoved = ShardBuffUtil.TryRemoveShardBuffsFromPlayer(user.LocalCharacter._Entity);
            if (wasABuffRemoved) {
                ServerChatUtils.SendSystemMessageToClient(VWorld.Server.EntityManager, user, $"Your shard buffs were removed by an admin ({ctx.User.CharacterName})");
            }
            
        }
        Plugin.Logger.LogInfo($"Shard buffs were removed from all players by {ctx.User.CharacterName} (steam#{ctx.User.PlatformId}).");
        ctx.Reply("Removed shard buffs from all players.");
    }

}
