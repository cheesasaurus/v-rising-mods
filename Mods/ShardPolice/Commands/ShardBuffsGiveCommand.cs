using ShardPolice.Utils;
using VampireCommandFramework;

namespace ShardPolice.Commands;


public class ShardBuffsGiveCommand {

    [Command("shard-buffs-give", shortHand: "sbg", description: "give shard buffs to self", adminOnly: true)]
    public void Execute(ChatCommandContext ctx) {
        ShardBuffUtil.GiveShardBuffsToPlayer(ctx.User.LocalCharacter._Entity);
        ctx.Reply("Gave all shard buffs to self");
    }

}
