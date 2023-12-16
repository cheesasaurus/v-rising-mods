using System;
using ShardPolice.Utils;
using VampireCommandFramework;

namespace ShardPolice.Commands;


public class DebugCommand {

    [Command("debug", description: "some debug thing", adminOnly: true)]
    public void Execute(ChatCommandContext ctx) {
        ctx.Reply($"DateTime.Now is: {DateTime.Now}");
    }

}
