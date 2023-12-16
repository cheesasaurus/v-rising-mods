
using System.Text;
using Bloodstone.API;
using CastleHeartPolice.CastleHeartScore;
using CastleHeartPolice.Config;
using CastleHeartPolice.Models;
using CastleHeartPolice.Utils;
using Unity.Entities;

namespace CastleHeartPolice.Services;

public class RulesService {

    public static RulesService Instance {get; private set; }

    public static void InitInstance(TerritoryScoresConfig territoryScoresConfig) {
        var strategyFactory = new CastleHeartScoreStrategyFactory(territoryScoresConfig);
        var maxCastleHeartScorePerClan = CastleHeartPoliceConfig.MaxCastleHeartScorePerClan.Value;
        var scoreStrategy = strategyFactory.Strategy(CastleHeartPoliceConfig.CastleHeartScoreStrategy.Value);
        Instance = new RulesService(maxCastleHeartScorePerClan, scoreStrategy);
    }

    private int MaxCastleHeartScorePerClan;
    private ICastleHeartScoreStrategy CastleHeartScoreStrategy;

    public RulesService(int maxCastleHeartScorePerClan, ICastleHeartScoreStrategy castleHeartScoreStrategy) {
        MaxCastleHeartScorePerClan = maxCastleHeartScorePerClan;
        CastleHeartScoreStrategy = castleHeartScoreStrategy;
    }

    public int HeartScore(Entity castleHeart) {
        return CastleHeartScoreStrategy.HeartScore(castleHeart);
    }

    public int TerritoryScore(CastleTerritoryInfo territoryInfo) {
        return CastleHeartScoreStrategy.TerritoryScore(territoryInfo);
    }

    public CheckRuleResult CheckRulePlaceCastleHeartInTerritory(Entity character, CastleTerritoryInfo territoryInfo) {
        var currentScore = 0;
        var teamHearts = CastleHeartUtil.FindCastleHeartsOfPlayerTeam(character);
        foreach (var heart in teamHearts) {
            currentScore += HeartScore(heart);
        }

        var territoryScore = TerritoryScore(territoryInfo);
        
        var result = CheckRuleResult.Allowed();
        if ((currentScore + territoryScore) > MaxCastleHeartScorePerClan) {
            var message = new StringBuilder();
            message.AppendLine($"Claiming this territory would put you and your clan over the allowed limit of {LabeledScore(MaxCastleHeartScorePerClan)}.");
            message.AppendLine($"Currently at:\t\t{LabeledScore(currentScore)}");
            message.AppendLine($"Territory value:\t{LabeledScore(territoryScore)}");
            message.AppendLine($"Total:\t\t\t{LabeledScore(currentScore + territoryScore)}");
            result.AddViolation(message.ToString());
        }
        return result;
    }

    public CheckRuleResult CheckRuleJoinClan(Entity character, Entity clan) {
        var playerScore = 0;
        var playerHearts = CastleHeartUtil.FindCastleHeartsOfPlayer(character);
        foreach (var heart in playerHearts) {
            playerScore += HeartScore(heart);
        }

        var clanScore = 0;
        var clanHearts = CastleHeartUtil.FindCastleHeartsOfClan(clan);
        foreach (var heart in clanHearts) {
            clanScore += HeartScore(heart);
        }

        var result = CheckRuleResult.Allowed();
        if ((playerScore + clanScore) > MaxCastleHeartScorePerClan) {
            var message = new StringBuilder();
            message.AppendLine($"Joining that clan would put you and your clan over the allowed limit of {LabeledScore(MaxCastleHeartScorePerClan)}.");
            message.AppendLine($"You:\t\t{LabeledScore(playerScore)}");
            message.AppendLine($"Them:\t{LabeledScore(clanScore)}");
            message.AppendLine($"Total:\t\t{LabeledScore(playerScore + clanScore)}");
            result.AddViolation(message.ToString());
        }
        return result;
    }

    public CheckRuleResult CheckRuleClaimCastleHeart(Entity character, Entity claimedHeart) {
        var currentScore = 0;
        var teamHearts = CastleHeartUtil.FindCastleHeartsOfPlayerTeam(character);
        foreach (var teamHeart in teamHearts) {
            if (claimedHeart.Equals(teamHeart)) {
                return CheckRuleResult.Allowed();
            }
            currentScore += HeartScore(teamHeart);
        }

        var claimedHeartScore = HeartScore(claimedHeart);

        var result = CheckRuleResult.Allowed();
        if ((currentScore + claimedHeartScore) > MaxCastleHeartScorePerClan) {
            var message = new StringBuilder();
            message.AppendLine($"Claiming this territory would put you and your clan over the allowed limit of {LabeledScore(MaxCastleHeartScorePerClan)}.");
            message.AppendLine($"Currently at:\t\t{LabeledScore(currentScore)}");
            message.AppendLine($"Territory value:\t{LabeledScore(claimedHeartScore)}");
            message.AppendLine($"Total:\t\t\t{LabeledScore(currentScore + claimedHeartScore)}");
            result.AddViolation(message.ToString());
        }
        return result;
    }

    public string LabeledScore(int score) {
        if (score == 1) {
            return $"{score} Point";
        }
        return $"{score} Points";
    }

}
