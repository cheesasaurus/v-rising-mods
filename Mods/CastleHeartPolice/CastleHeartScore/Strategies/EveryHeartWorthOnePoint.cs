using CastleHeartPolice.Models;
using Unity.Entities;

namespace CastleHeartPolice.CastleHeartScore.Strategies;

public class EveryHeartWorthOnePoint : ICastleHeartScoreStrategy {
    public override int HeartScore(Entity entity) {
        return 1;
    }

    public override int TerritoryScore(CastleTerritoryInfo territory) {
        return 1;
    }
}