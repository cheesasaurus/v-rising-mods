using cheesasaurus.VRisingMods.SystemsDumper.CodeGeneration;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Gameplay.Scripting.Systems;
using ProjectM.Gameplay.Systems;
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
        var queryCodeGen = new EntityQueryCodeGenerator();

        var systemHandle = world.GetExistingSystem<DealDamageSystem>();
        var systemState = world.Unmanaged.ResolveSystemState(systemHandle);
        var queries = systemState->GetEntityQueries(); // does not crash
        LogUtil.LogInfo($"queries.Length: {queries.Length}"); // does not crash

        for (var i = 0; i < queries.Length; i++)
        {
            var query = queries[i]; // does not crash
            var _ = query.IsCacheValid; // magically prevents crashes?
            var queryDesc = query.GetEntityQueryDesc(); // does not crash

            var namedQuery = new NamedEntityQuery("ooga booga", query);

            LogUtil.LogDebug(queryCodeGen.Snippet_CreateQueryFrom_QueryDesc(namedQuery));
        }
        
        ctx.Reply($"Did the debug thing");
    }
    
}
