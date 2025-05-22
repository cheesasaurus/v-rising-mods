using ShardPolice.Utils;
using VampireCommandFramework;
using VRisingMods.Core.Buff;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace ShardPolice.Commands;


public class ShardResetRelicsCommand {

    [Command("shard-reset-relics", description: "reset all soul shards (the placeable relics, not the wearable amulets)", adminOnly: true)]
    public void Execute(ChatCommandContext ctx) {
        foreach (var user in UserUtil.FindAllUsers()) {
            ShardRelicBuffUtil.TryRemoveShardBuffsFromPlayer(user.User.LocalCharacter._Entity);
        }
        ShardRelicItemUtil.RemovePlacedShards();
        ShardRelicItemUtil.PrepareShardItemsToDespawn();
        ChatUtil.SendSystemMessageToAllClients("Soul Shards (placeable relics) have been reset!");
        LogUtil.LogMessage($"Shards (placeable relics) were reset by {ctx.User.CharacterName} (steam#{ctx.User.PlatformId})");
    }

}
