using BepInEx.Configuration;

namespace ShardPolice;

public static class ShardPoliceConfig {
    public static ConfigEntry<bool> LimitShardBuffsToOnlyOneAtATime { get; private set; }
    public static ConfigEntry<bool> PreventShardOwnersMovingPlacedShardDuringRaidHours { get; private set; }

    public static void Init(ConfigFile config) {

        LimitShardBuffsToOnlyOneAtATime = config.Bind<bool>(
            section: "Shard Buffs",
            key: "LimitShardBuffsToOnlyOneAtATime",
            defaultValue: true,
            description: "Whether or not to limit each player to only 1 shard buff at a time"
        );

        PreventShardOwnersMovingPlacedShardDuringRaidHours = config.Bind<bool>(
            section: "Shard Movement",
            key: "PreventShardOwnersMovingPlacedShardDuringRaidHours",
            defaultValue: true,
            description: "Whether or not to prevent shard owners from moving their placed shard during raid hours"
        );
        
    }
    
}
