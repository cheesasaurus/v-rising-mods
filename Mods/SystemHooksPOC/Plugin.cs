using System;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using ProjectM.Gameplay.Systems;
using SystemHooksPOC.Patches;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace SystemHooksPOC;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
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

        HookManager.Initialize();

        // register hooks
        HookManager.RegisterHook_System_OnUpdate_Prefix<DealDamageSystem>(MyHook);
    }

    public override bool Unload()
    {
        HookManager.UnInitialize();
        CommandRegistry.UnregisterAssembly();
        PerformanceRecorderSystemPatch.UnInitialize();
        _harmony?.UnpatchSelf();
        return true;
    }


    private TimeSpan fiveSeconds = new TimeSpan(hours: 0, minutes: 0, seconds: 5);
    private DateTime nextTime = DateTime.MinValue;

    private bool MyHook()
    {
        if (DateTime.Now < nextTime)
        {
            return true;
        }
        nextTime = DateTime.Now.Add(fiveSeconds);
        LogUtil.LogInfo($"[{DateTime.Now}] MyHook executing. (debounce 5 seconds)");
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
    // /// .systemhookspoc-example "some quoted string" 1 1.5
    // /// .systemhookspoc-example boop 21232
    // /// .systemhookspoc-example boop-boop
    // ///</remarks>
    // [Command("systemhookspoc-example", description: "Example command from systemhookspoc", adminOnly: true)]
    // public void ExampleCommand(ICommandContext ctx, string someString, int num = 5, float num2 = 1.5f)
    // { 
    //     ctx.Reply($"You passed in {someString} and {num} and {num2}");
    // }
}
