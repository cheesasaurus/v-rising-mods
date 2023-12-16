using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using CastleHeartPolice.Config;
using CastleHeartPolice.Services;
using HarmonyLib;
using VampireCommandFramework;

namespace CastleHeartPolice;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
[BepInDependency("gg.deca.Bloodstone")]
[Bloodstone.API.Reloadable]
public class Plugin : BasePlugin
{
    Harmony _harmony;
    public static ManualLogSource Logger;

    public override void Load()
    {
        // Plugin startup logic
        Logger = Log;
        CastleHeartPoliceConfig.Init(Config);
        var territoryScoresConfig = TerritoryScoresConfig.Init("territoryScores.json");
        RulesService.InitInstance(territoryScoresConfig);
        Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} version {MyPluginInfo.PLUGIN_VERSION} is loaded!");

        // Harmony patching
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        // Register all commands in the assembly with VCF
        CommandRegistry.RegisterAll();
    }

    public override bool Unload()
    {
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
    // /// .castleheartpolice-example "some quoted string" 1 1.5
    // /// .castleheartpolice-example boop 21232
    // /// .castleheartpolice-example boop-boop
    // ///</remarks>
    // [Command("castleheartpolice-example", description: "Example command from castleheartpolice", adminOnly: true)]
    // public void ExampleCommand(ICommandContext ctx, string someString, int num = 5, float num2 = 1.5f)
    // { 
    //     ctx.Reply($"You passed in {someString} and {num} and {num2}");
    // }
}
