using VampireCommandFramework;

namespace cheesasaurus.VRisingMods.SystemsDumper.Commands;

[CommandGroup("DumpSystems", "ds")]
internal static class DumpCommands_Server
{

    [Command("UpdateTree", "ut", description: "Dumps ECS system update hierarchies to files (per world)", adminOnly: true)]
    public static void DumpSystemsUpdateTrees(ChatCommandContext ctx)
    {
        var dir = Core.DumpService.DumpSystemsUpdateTrees();
        ctx.Reply($"Dumped system hierarchy files to folder {dir}");
    }
    
}
