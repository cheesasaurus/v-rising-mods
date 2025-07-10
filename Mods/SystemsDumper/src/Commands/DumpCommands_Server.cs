using Il2CppInterop.Runtime;
using Unity.Collections;
using Unity.Entities;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Commands;

[CommandGroup("DumpSystems", "ds")]
internal static class DumpCommands_Server
{

    [Command("UpdateTree", "ut", description: "Dumps ECS system update hierarchies to files (per world)", adminOnly: true)]
    public static void DumpSystemsUpdateTrees(ChatCommandContext ctx)
    {
        var filePattern = Core.DumpService.DumpSystemsUpdateTrees();
        ctx.Reply($"Dumped system update tree files as {filePattern}");
    }

    [Command("EntityQueries", "eq", description: "Dumps ECS system entity queries", adminOnly: true)]
    public static void DumpSystemsEntityQueries(ChatCommandContext ctx)
    {
        var filePattern = Core.DumpService.DumpSystemsEntityQueries();
        ctx.Reply($"Dumped system entity query files as {filePattern}");
    }

    [Command("code", "c", description: "Generates code snippets for each system", adminOnly: true)]
    public static void DumpSystemsCodeGen(ChatCommandContext ctx)
    {
        var filePattern = Core.DumpService.DumpSystemsCodeGen();
        ctx.Reply($"Generated code snippet files as {filePattern}");
    }

    // todo: remove
    [Command("debug", "d", description: "debug query builder", adminOnly: true)]
    unsafe public static void Debug(ChatCommandContext ctx)
    {
        var entityManager = WorldUtil.Server.EntityManager;

        var component = new ComponentType(Il2CppType.Of<ProjectM.Network.FromCharacter>(), ComponentType.AccessMode.ReadOnly);

        var _EventQuery = new EntityQueryBuilder(Allocator.Temp)
            .WithAll(&component, 1)
            .AddAll(new(Il2CppType.Of<ProjectM.Network.UnlockResearchEvent>(), ComponentType.AccessMode.ReadOnly))
            //.WithAll<ProjectM.Network.UnlockResearchEvent>() // doesn't work
            .Build(entityManager);


        var entities = _EventQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            //DebugUtil.LogComponentTypes(entity);
        }

        ctx.Reply($"did something");
    }

    // todo: remove    
    [Command("debug2", "dd", description: "debug query desc", adminOnly: true)]
    public static void Debug2(ChatCommandContext ctx)
    {
        var entityManager = WorldUtil.Server.EntityManager;

        var _EventQuery = entityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] {
                ComponentType.ReadOnly<ProjectM.UnitSpawnerstation>(),
                ComponentType.ReadOnly<ProjectM.EditableTileModel>(),
                ComponentType.ReadOnly<ProjectM.CastleWorkstation>(),
                ComponentType.ReadOnly<ProjectM.InventoryOwner>(),
                ComponentType.ReadOnly<ProjectM.StationBonusBuffer>(),
                ComponentType.ReadOnly<ProjectM.RefinementstationRecipesBuffer>(),
            },
            Options = EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludeDisabled | EntityQueryOptions.IncludeAll,
        });


        var entities = _EventQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            //DebugUtil.LogComponentTypes(entity);
        }
        
        ctx.Reply($"did something2");
    }
    
}
