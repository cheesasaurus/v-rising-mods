using System;
using cheesasaurus.VRisingMods.EventScheduler;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

public class Commands
{

    [Command("testRunEvent", description: "immediately test run a scheduled event", adminOnly: true)]
    public void TestRunEventCommand(ICommandContext ctx, string eventId)
    {
        if (Core.EventsConfig is null)
        {
            ctx.Reply("<color=red>There was an error parsing the events config</color>");
            return;
        }

        if (Core.EventRunner is null)
        {
            ctx.Reply("<color=red>There was an error setting up the event runner</color>");
            return;
        }

        if (!Core.EventsConfig.TryGetScheduledEvent(eventId, out var scheduledEvent))
        {
            ctx.Reply($"<color=red>No event found with id \"{eventId}\"</color>");
            return;
        }
        ctx.Reply($"Scheduled an immediate test run for event \"{scheduledEvent.EventId}\"");
        LogUtil.LogMessage($"{DateTime.Now}: {ctx.Name} scheduled an immediate test run for event {scheduledEvent.EventId}");
        Core.EventRunner.PrepareTestRun(scheduledEvent);
    }
    
}