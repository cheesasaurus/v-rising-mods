using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.BuildCostsPOC.Commands;

/// <summary>
/// This changes the building costs server-side. The new costs apply to both placing and dismantling tile models.
/// Does not persist across server restarts.
/// TODO: how to propagate prefab changes to the client?
/// </summary>
public class DebugModifyBlueprintCommand
{
    private readonly World _world;
    private readonly EntityManager _entityManager;
    private readonly GameDataSystem _gameDataSystem;

    private readonly PrefabCollectionSystem _prefabCollectionSystem;
    private PrefabLookupMap _prefabLookupMap => _prefabCollectionSystem._PrefabLookupMap;

    private readonly ServerGameSettingsSystem _serverGameSettingsSystem;
    private ServerGameSettings _serverGameSettings => _serverGameSettingsSystem._Settings;

    public DebugModifyBlueprintCommand()
    {
        _world = WorldUtility.FindServerWorld();
        _entityManager = _world.EntityManager;
        _gameDataSystem = _world.GetExistingSystemManaged<GameDataSystem>();
        _prefabCollectionSystem = _world.GetExistingSystemManaged<PrefabCollectionSystem>();
        _serverGameSettingsSystem = _world.GetExistingSystemManaged<ServerGameSettingsSystem>();
    }

    [Command("debugmbp", description: "modify blueprint building costs", adminOnly: true, usage: ".debugmbp -1259825508")]
    public void Execute(ICommandContext ctx, int prefabGuidHash)
    {
        var prefabGuid = new PrefabGUID(prefabGuidHash);
        if (!_gameDataSystem.BlueprintHashLookupMap.TryGetValue(prefabGuid, out var blueprintData))
        {
            ctx.Reply($"No blueprint data found for prefab with hash {prefabGuidHash}");
            return;
        }

        var newConfiguredCosts = new List<ConfiguredCost>()
        {
            new("Item_Ingredient_Wood_Standard", 23),
            new("Item_Ingredient_Plant_PlantFiber", 42),
        };

        if (!TryParseCosts(newConfiguredCosts, out var newCosts))
        {
            ctx.Reply($"The new configured costs were not valid.");
            return;
        }

        var costFactor = _serverGameSettings.BuildCostModifier;
        newCosts = newCosts.Select(c => c * costFactor).ToList();

        if (!TryModifyBuildingCosts(blueprintData.Entity, newCosts))
        {
            ctx.Reply($"Failed to modify building costs.");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Changed cost of blueprint {_prefabLookupMap.GetName(prefabGuid)} to:");
        foreach (var cost in newCosts)
        {
            sb.AppendLine($"  {_prefabLookupMap.GetName(cost.PrefabGUID)} x{cost.Amount}");
        }
        ctx.Reply(sb.ToString());
    }

    private class ConfiguredCost
    {
        public ConfiguredCost(string prefabName, int amount)
        {
            PrefabName = prefabName;
            Amount = amount;
        }

        public string PrefabName { get; set; }
        public int Amount { get; set; }
    }

    private class Cost
    {
        public PrefabGUID PrefabGUID { get; set; }
        public int Amount { get; set; }

        public static Cost operator *(Cost cost, double factor)
        {
            return new Cost()
            {
                PrefabGUID = cost.PrefabGUID,
                Amount = Convert.ToInt32(cost.Amount * factor),
            };
        }
    }

    private bool TryParseCosts(List<ConfiguredCost> configuredCosts, out List<Cost> costs)
    {
        costs = [];
        foreach (var configuredCost in configuredCosts)
        {
            if (!_prefabLookupMap.TryGetPrefabGuidWithName(configuredCost.PrefabName, out var prefabGuid))
            {
                LogUtil.LogWarning($"Failed to find prefab named '{configuredCost.PrefabName}'");
                return false;
            }

            if (!_gameDataSystem.ItemHashLookupMap.ContainsKey(prefabGuid))
            {
                LogUtil.LogWarning($"'{configuredCost.PrefabName}' is not an item, and thus cannot be used as a cost!");
                return false;
            }

            costs.Add(new Cost
            {
                PrefabGUID = prefabGuid,
                Amount = configuredCost.Amount,
            });
        }
        return true;
    }

    private bool TryModifyBuildingCosts(Entity blueprintEntity, List<Cost> newCosts)
    {
        if (!_entityManager.TryGetBuffer<BlueprintRequirementBuffer>(blueprintEntity, out var blueprintRequirements))
        {
            return false;
        }

        blueprintRequirements.Clear();
        foreach (var cost in newCosts)
        {
            blueprintRequirements.Add(new BlueprintRequirementBuffer
            {
                PrefabGUID = cost.PrefabGUID,
                Amount = cost.Amount,
            });
        }

        return true;
    }

}