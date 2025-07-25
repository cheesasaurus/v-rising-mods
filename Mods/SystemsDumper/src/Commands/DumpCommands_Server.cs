
using VampireCommandFramework;

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
    
}
