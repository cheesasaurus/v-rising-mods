using Gameplay.Scripting.Systems;
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

    [Command("Code", "c", description: "Generates code snippets for each system", adminOnly: true)]
    public static void DumpSystemsCodeGen(ChatCommandContext ctx)
    {
        var filePattern = Core.DumpService.DumpSystemsCodeGen();
        ctx.Reply($"Generated code snippet files as {filePattern}");
    }

    // todo: remove
    [Command("Debug", "d", description: "Does some debug thing", adminOnly: true)]
    unsafe public static void DumpSystemsDebug(ChatCommandContext ctx)
    {
        var world = WorldUtil.Server;
        var systemHandle = world.GetExistingSystem<UpdateDisabledCarriageColliderSystem>();
        var unsafeRef = world.Unmanaged.GetUnsafeSystemRef<UpdateDisabledCarriageColliderSystem>(systemHandle);

        var query = unsafeRef.__query_1612747886_0;
        LogUtil.LogInfo($"is query null: {&query is null}");
        LogUtil.LogInfo($"IsQueryValid: {world.EntityManager.IsQueryValid(query)}");

        // var queryDesc = query.GetEntityQueryDesc(); // crashes
        // var queryDescs = query.GetQueryTypes(); // crashes
        // var cachedState = query._CachedState; // does not crash
        // var isCacheValid = query.IsCacheValid; // crashes
        var queryImpl = *query.__impl; // does not crash
        //var access = *queryImpl._Access; // crashes


        var systemState = world.Unmanaged.ResolveSystemStateRef(systemHandle);
        // var queries = systemState.GetEntityQueries(); // crashes
        // var queries = systemState.EntityQueries; // does not crash. but Length is 0.
        var queries = systemState.m_EntityQueries; // does not crash. but crashes when trying to access a query
        LogUtil.LogInfo($"queries count: {queries.Length}");

        if (queries.Ptr is null)
        {
            LogUtil.LogInfo("null pointer");
        }
        else
        {
            // var first = *queries.Ptr; // crashes
        }

        for (var i = 0; i < queries.Length; i++)
        {
            // var q = queries[0]; // crashes
        }

        //foreach (var q in queries)
        //{
        //    //LogUtil.LogInfo($"IsQueryValid: {world.EntityManager.IsQueryValid(query)}");
        //}

        //foreach (var world in World.s_AllWorlds)
        //{
        //    var systemModels = Core.EcsSystemMetadataService.CollectSystemMetadata(world);
        //}
        ctx.Reply($"Did the debug thing");
    }
    
}
