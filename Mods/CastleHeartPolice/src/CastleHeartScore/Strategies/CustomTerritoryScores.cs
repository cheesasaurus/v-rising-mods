using CastleHeartPolice.Config;
using CastleHeartPolice.Models;
using CastleHeartPolice.Utils;
using ProjectM.CastleBuilding;
using Unity.Entities;

namespace CastleHeartPolice.CastleHeartScore.Strategies;

public class CustomTerritoryScores : ICastleHeartScoreStrategy {
    TerritoryScoresConfig TerritoryScoresConfig;
    public CustomTerritoryScores(TerritoryScoresConfig territoryScoresConfig) {
        TerritoryScoresConfig = territoryScoresConfig;
    }
    
    public override int HeartScore(Entity entity) {
        if (CastleTerritoryUtil.TryFindTerritoryOfCastleHeart(entity, out var territoryInfo)) {
            return TerritoryScoresConfig.ScoreForTerritory(territoryInfo.TerritoryId);
        }
        return 1;        
    }

    public override int TerritoryScore(CastleTerritoryInfo territory) {
        return TerritoryScoresConfig.ScoreForTerritory(territory.TerritoryId);
    }
}