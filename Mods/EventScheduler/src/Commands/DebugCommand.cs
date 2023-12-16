using System;
using Bloodstone.API;
using ProjectM;
using ProjectM.Network;
using VampireCommandFramework;
using VampireCommandFramework.Breadstone;
using VRisingMods.Core.Player;
using VRisingMods.Core.Utilities;

namespace EventScheduler.Commands;


public class DebugCommand {

    [Command("debug", description: "some debug thing", adminOnly: true)]
    public void Execute(ChatCommandContext ctx) {
        ctx.Reply($"DateTime.Now is: {DateTime.Now}");
    }

}
