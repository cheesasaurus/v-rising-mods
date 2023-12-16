using BepInEx.Configuration;
using CastleHeartPolice.CastleHeartScore.Strategies;

namespace CastleHeartPolice.Config;

public static class CastleHeartPoliceConfig {
    public static ConfigEntry<int> MaxCastleHeartScorePerClan { get; private set; }
    public static ConfigEntry<string> CastleHeartScoreStrategy { get; private set; }

    public static void Init(ConfigFile config) {

        MaxCastleHeartScorePerClan = config.Bind<int>(
            section: "General",
            key: "MaxCastleHeartScorePerClan",
            defaultValue: 1,
            description: "The value of castle hearts owned by a single clan may not exceed this score."
        );

        CastleHeartScoreStrategy = config.Bind<string>(
            section: "General",
            key: "CastleHeartScoreStrategy",
            defaultValue: nameof(EveryHeartWorthOnePoint),
            description: $"Determines the value of each castle heart. ({nameof(EveryHeartWorthOnePoint)} | {nameof(CustomTerritoryScores)})"
        );
        
    }
    
}
