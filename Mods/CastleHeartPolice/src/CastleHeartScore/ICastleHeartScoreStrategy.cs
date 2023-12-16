using Unity.Entities;
using VRisingMods.Core.CastleTerritory.Models;

namespace CastleHeartPolice.CastleHeartScore;

// bloodstone doesn't seem to support interfaces :(
public abstract class ICastleHeartScoreStrategy {
    public abstract int HeartScore(Entity entity);
    public abstract int TerritoryScore(CastleTerritoryInfo territory);
}