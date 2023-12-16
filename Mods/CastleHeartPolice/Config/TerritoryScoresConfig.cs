using System.Collections.Generic;
using System.Text.Json;

namespace CastleHeartPolice.Config;


public class TerritoryScoresConfig : AbstractJsonConfig {

    private Dictionary<int, int> ScoreByTerritoryId = new();
    private int DefaultScore = 1;

    public TerritoryScoresConfig(string filepath) : base(filepath) {

    }

    public int ScoreForTerritory(int territoryId) {
        if (ScoreByTerritoryId.TryGetValue(territoryId, out var score)) {
            return score;
        }
        return DefaultScore;
    }

    public override string ToJson() {
        var options = new JsonSerializerOptions {
            WriteIndented = true,
        };
        return JsonSerializer.Serialize(ScoreByTerritoryId, options);
    }

    protected override void InitDefaults() {
        var territoryCount = 140;
        for (var id = 0; id < territoryCount; id++) {
            ScoreByTerritoryId.Add(id, DefaultScore);
        }
    }

    protected override void InitFromJson(string json) {
        ScoreByTerritoryId = JsonSerializer.Deserialize<Dictionary<int, int>>(json);
    }

    public static TerritoryScoresConfig Init(string filename) {
        return Init<TerritoryScoresConfig>(filename);
    }

}
