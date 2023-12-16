using Bloodstone.API;
using ProjectM;
using ShardPolice.Utils;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace ShardPolice.Commands;


public class ShardsResetCommand {

    [Command("shards-reset", description: "reset all shards", adminOnly: true)]
    public void Execute(ChatCommandContext ctx) {
        foreach (var user in UserUtil.FindAllUsers()) {
            ShardBuffUtil.TryRemoveShardBuffsFromPlayer(user.LocalCharacter._Entity);
        }
        ShardItemUtil.RemovePlacedShardsAndDropNearCharacterToDespawn(ctx.User.LocalCharacter._Entity);
        ShardItemUtil.PrepareShardItemsToDespawn();
        ServerChatUtils.SendSystemMessageToAllClients(VWorld.Server.EntityManager, "Shards have been reset!");
        LogUtil.LogMessage($"Shards were reset by {ctx.User.CharacterName} (steam#{ctx.User.PlatformId})");
    }

}
