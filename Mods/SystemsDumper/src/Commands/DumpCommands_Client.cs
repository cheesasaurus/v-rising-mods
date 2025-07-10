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

            case ".dumpsystems code":
            case ".dumpsystems c":
            case ".ds code":
            case ".ds c":
                CommandDumpSystemsCodeGen();
                break;

            default:
                break;
        }
    }

    public static void CommandDumpSystemsUpdateTrees()
    {
        LogUtil.LogInfo("Dumping systems");
        Core.DumpService.DumpSystemsUpdateTrees();
    }

    public static void CommandDumpSystemsCodeGen()
    {
        LogUtil.LogInfo("Dumping systems");
        Core.DumpService.DumpSystemsCodeGen();
    }
    
}
