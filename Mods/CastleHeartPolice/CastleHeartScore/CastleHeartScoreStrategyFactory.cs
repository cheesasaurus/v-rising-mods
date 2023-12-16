using CastleHeartPolice.CastleHeartScore.Strategies;
using CastleHeartPolice.Config;

namespace CastleHeartPolice.CastleHeartScore;

public class CastleHeartScoreStrategyFactory {

    private TerritoryScoresConfig TerritoryScoresConfig;

    public CastleHeartScoreStrategyFactory(TerritoryScoresConfig territoryScoresConfig) {
        TerritoryScoresConfig = territoryScoresConfig;
    }

    public ICastleHeartScoreStrategy Strategy(string strategyString) {
        switch (strategyString) {
            case nameof(CustomTerritoryScores):
                return new CustomTerritoryScores(TerritoryScoresConfig);
            case nameof(EveryHeartWorthOnePoint):
                return new EveryHeartWorthOnePoint();
            default:
                return new EveryHeartWorthOnePoint();
        }
    }

}
