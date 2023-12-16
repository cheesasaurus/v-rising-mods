using System;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using Bloodstone.Hooks;
using EventScheduler.Config;
using HarmonyLib;
using VampireCommandFramework;
using VRisingMods.Core.Chat;
using VRisingMods.Core.Utilities;

namespace EventScheduler;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
[BepInDependency("gg.deca.Bloodstone")]
[Bloodstone.API.Reloadable]
public class Plugin : BasePlugin
{
    Harmony _harmony;

    public override void Load()
    {
        // Plugin startup logic
        LogUtil.Init(Log);
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        var eventsConfig = EventsConfig.Init(MyPluginInfo.PLUGIN_GUID, "events.jsonc");

        // Harmony patching
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        // Register all commands in the assembly with VCF
        CommandRegistry.RegisterAll();

        GameFrame.OnUpdate += Tick;
    }

    private static int lastSecondLogged = 0;

    private void Tick() {
        var now = DateTime.Now;
        var second = now.Second;
        if (second != lastSecondLogged && second % 5 == 0) {
            lastSecondLogged = second;
            // Log.LogMessage($"5 seconds passed. ${now}");
            // ChatUtil.ForgeMessage("Dingus", ".shard-buffs-remove-everyone", ProjectM.Network.ChatMessageType.Global);
        }
    }

    public override bool Unload()
    {
        GameFrame.OnUpdate -= Tick;
        CommandRegistry.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }

    // // Uncomment for example commmand or delete

    // /// <summary> 
    // /// Example VCF command that demonstrated default values and primitive types
    // /// Visit https://github.com/decaprime/VampireCommandFramework for more info 
    // /// </summary>
    // /// <remarks>
    // /// How you could call this command from chat:
    // ///
    // /// .eventscheduler-example "some quoted string" 1 1.5
    // /// .eventscheduler-example boop 21232
    // /// .eventscheduler-example boop-boop
    // ///</remarks>
    // [Command("eventscheduler-example", description: "Example command from eventscheduler", adminOnly: true)]
    // public void ExampleCommand(ICommandContext ctx, string someString, int num = 5, float num2 = 1.5f)
    // { 
    //     ctx.Reply($"You passed in {someString} and {num} and {num2}");
    // }
}
