using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Commands;

internal static class DumpCommands_Client
{
    public static void HandleChatMessageSubmit(string message)
    {
        switch (message.ToLowerInvariant())
        {
            case ".dumpsystems updatetree":
            case ".dumpsystems ut":
            case ".ds updatetree":
            case ".ds ut":
                CommandDumpSystemsUpdateTrees();
                break;

            default:
                break;
        }
    }

    public static void CommandDumpSystemsUpdateTrees()
    {
        LogUtil.LogInfo("Dumping systems");
        var dir = Core.DumpService.DumpSystemsUpdateTrees();
    }
    
}
