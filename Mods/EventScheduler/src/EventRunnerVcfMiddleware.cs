using System.Reflection;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.EventScheduler;

class EventRunnerVcfMiddleware(EventRunner eventRunner) : CommandMiddleware
{
    private EventRunner _eventRunner = eventRunner;

    public override bool CanExecute(ICommandContext ctx, CommandAttribute command, MethodInfo method)
    {
        LogUtil.LogDebug($"VCF middleware running, checking name: {ctx.Name}");
        var chatCtx = (ChatCommandContext)ctx;
        if (_eventRunner.SpawnedChatCommandsThisTick && _eventRunner.IsCommandExecutor(chatCtx.User))
        {
            // Property or indexer 'ChatCommandContext.IsAdmin' cannot be assigned to -- it is read only CS0200
            // Which makes this whole idea moot :(
            // chatCtx.IsAdmin = true;
            LogUtil.LogDebug("Would mark the user as admin here");
        }
        // always return true. VCF comes with a BasicAdminCheck middleware which will run after this.
        // (or other plugins will add their own middleware)
        return true;


        // return true if user is the configured command executer for the eventrunner.
        // also set ctx.IsAdmin to true if that's the case, in case other middlewares would interfere.
        //
        // And only do this if command runner is supposed to execute commands this tick.
        // (set a flag when creating the chat messages. And clear the flag after the chat message system ran.)

        // register an instance of this middleware in plugin load method, with CommandRegistry.Middlewares.Add
        // And in the plugin unload method, remove that instance of the middleware somehow.
        // I think Middlewares is just a List, needs investigation.
    }

}