using BepInEx;
using BepInEx.Unity.IL2CPP;
using Explorer.UI;
using HarmonyLib;
using UnityEngine;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace Explorer;

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

        // Harmony patching
        _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());

        // Register all commands in the assembly with VCF
        CommandRegistry.RegisterAll();

        // init UI
        initUI();
    }

    private void initUI() {
        float startupDelay = 1f;
        UniverseLib.Config.UniverseLibConfig config = new();
        UniverseLib.Universe.Init(startupDelay, UIOnInitialized, UILogHandler, config);
    }

    void UIOnInitialized() {
        // todo: not sure if this is the right place
        Universe_OnInitialized();
    }

    public static UniverseLib.UI.UIBase UiBase { get; private set; }
    void Universe_OnInitialized()
    {
        UiBase = UniverseLib.UI.UniversalUI.RegisterUI("my.unique.ID", UiUpdate);
        MyPanel myPanel = new(UiBase);
    }

    void UiUpdate()
    {
        // Called once per frame when your UI is being displayed.
    }

    void UILogHandler(string message, LogType type) {

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
    // /// .explorer-example "some quoted string" 1 1.5
    // /// .explorer-example boop 21232
    // /// .explorer-example boop-boop
    // ///</remarks>
    // [Command("explorer-example", description: "Example command from explorer", adminOnly: true)]
    // public void ExampleCommand(ICommandContext ctx, string someString, int num = 5, float num2 = 1.5f)
    // { 
    //     ctx.Reply($"You passed in {someString} and {num} and {num2}");
    // }
}
