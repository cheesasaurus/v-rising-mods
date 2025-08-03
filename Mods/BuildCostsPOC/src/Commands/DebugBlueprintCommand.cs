using System;
using System.Text;
using Cpp2IL.Core.Extensions;
using ProjectM;
using Stunlock.Core;
using Unity.Entities;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.BuildCostsPOC.Commands;

public class DebugBlueprintCommand
{
    private readonly World _world;
    private readonly EntityManager _entityManager;
    private readonly GameDataSystem _gameDataSystem;

    private readonly PrefabCollectionSystem _prefabCollectionSystem;
    private PrefabLookupMap _prefabLookupMap => _prefabCollectionSystem._PrefabLookupMap;

    ServerGameSettingsSystem _serverGameSettingsSystem;
    private ServerGameSettings _serverGameSettings => _serverGameSettingsSystem._Settings;

    public DebugBlueprintCommand()
    {
        _world = WorldUtility.FindServerWorld();
        _entityManager = _world.EntityManager;
        _gameDataSystem = _world.GetExistingSystemManaged<GameDataSystem>();
        _prefabCollectionSystem = _world.GetExistingSystemManaged<PrefabCollectionSystem>();
        _serverGameSettingsSystem = _world.GetExistingSystemManaged<ServerGameSettingsSystem>();
    }

    [Command("debugbp", description: "log blueprint stuff for a given blueprint", adminOnly: true, usage: ".debugbp -1259825508")]
    public void Execute(ICommandContext ctx, int prefabGuidHash)
    {
        var prefabGuid = new PrefabGUID(prefabGuidHash);
        if (!_gameDataSystem.BlueprintHashLookupMap.TryGetValue(prefabGuid, out var blueprintData))
        {
            ctx.Reply($"No blueprint data found for prefab with hash {prefabGuidHash}");
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("==============================================================================");
        sb.AppendLine(CodeGen_NamedPrefabGuid(prefabGuid));

        sb.AppendLine("Component types:");
        sb.AppendLine(Indent(1, ListComponentTypes(blueprintData.Entity)));

        sb.AppendLine($"Global BuildCostModifier: {_serverGameSettings.BuildCostModifier}");

        sb.AppendLine("Blueprint requirements:");
        sb.AppendLine(Indent(1, ListBlueprintRequirements(blueprintData.Entity)));
        // nb: The requirements seem to already have the global BuildCostModifier factored in.
        //     For example, with a modifier of 0.0, the requirement amounts are all 0. 

        sb.AppendLine("-------------------------------------------");

        LogUtil.LogInfo(sb.ToString());

        ctx.Reply($"done");
    }

    private string CodeGen_NamedPrefabGuid(PrefabGUID prefabGuid)
    {
        if (!_prefabLookupMap.TryGetName(prefabGuid, out var name))
        {
            name = $"NameNotFound_{prefabGuid}";
        }
        return $"PrefabGUID {name} = new PrefabGUID({prefabGuid.GuidHash});";
    }

    private string Indent(int level, string multiLineString, int spacesPerIndent = 2)
    {
        var leftPadding = " ".Repeat(level * spacesPerIndent);
        return leftPadding + multiLineString.Replace("\n", $"\n{leftPadding}");
    }

    private string ListComponentTypes(Entity entity)
    {
        var sb = new StringBuilder();
        foreach (var componentType in _entityManager.GetComponentTypes(entity))
        {
            sb.AppendLine(componentType.ToString());
        }
        return sb.ToString();
    }

    private string ListBlueprintRequirements(Entity entity)
    {
        if (!_entityManager.TryGetBuffer<BlueprintRequirementBuffer>(entity, out var blueprintRequirements))
        {
            return "(none)";
        }
        var sb = new StringBuilder();
        for (var i = 0; i < blueprintRequirements.Length; i++)
        {
            var bpReq = blueprintRequirements[i];
            sb.AppendLine($"BlueprintRequirementBuffer[{i}]:");
            sb.AppendLine($"  PrefabGUID: {bpReq.PrefabGUID.GuidHash} | {_prefabLookupMap.GetName(bpReq.PrefabGUID)}");
            sb.AppendLine($"  Amount: {bpReq.Amount}");
        }
        return sb.ToString();
    }

}