using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;
using Bloodstone.API;

namespace SystemsPOC;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
[BepInDependency("gg.deca.Bloodstone")]
[Bloodstone.API.Reloadable]
public class Plugin : BasePlugin
{
    Harmony _harmony;

    Unity.Entities.SystemHandle mySystem;

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

        // register systems
        RegisterSystems();
       
    }

    public void RegisterSystems() {
        var systemType = Il2CppSystem.Type.GetType(typeof(MySystem).FullName);
        mySystem = VWorld.Server.CreateSystem(systemType);

        // TODO: need to register an il2cppSystem type somehow
        // [Error  :Bloodstone] Plugin SystemsPOC.Plugin threw an exception during initialization:
        // [Error  :Bloodstone] Il2CppInterop.Runtime.Il2CppException: System.ArgumentException: Type null couldn't be found in the SystemRegistry. (This is likely a bug in an ILPostprocessor.)
        // --- BEGIN IL2CPP STACK TRACE ---
        // System.ArgumentException: Type null couldn't be found in the SystemRegistry. (This is likely a bug in an ILPostprocessor.)
        // at Unity.Entities.WorldUnmanagedImpl.CreateUnmanagedSystemInternal (Unity.Entities.World self, System.Int32 structSize, System.Int64 typeHash, System.Int32 typeIndex, System.Void*& systemPtr, System.Boolean callOnCreate) [0x00000] in <00000000000000000000000000000000>:0
        // at Unity.Entities.WorldUnmanagedImpl.CreateUnmanagedSystem (Unity.Entities.SystemTypeIndex t, System.Int64 typeHash, System.Boolean callOnCreate) [0x00000] in <00000000000000000000000000000000>:0
        // at Unity.Entities.WorldUnmanagedImpl.GetOrCreateUnmanagedSystem (Unity.Entities.SystemTypeIndex t, System.Boolean callOnCreate) [0x00000] in <00000000000000000000000000000000>:0
        // at Unity.Entities.WorldUnmanaged.GetOrCreateUnmanagedSystem (Unity.Entities.SystemTypeIndex unmanagedType) [0x00000] in <00000000000000000000000000000000>:0
        // at Unity.Entities.World.CreateSystem (System.Type type) [0x00000] in <00000000000000000000000000000000>:0
        // --- END IL2CPP STACK TRACE ---
        // 
        // at Il2CppInterop.Runtime.Il2CppException.RaiseExceptionIfNecessary(IntPtr returnedException) in C:\git\v-rising\Il2CppInterop\Il2CppInterop.Runtime\Il2CppException.cs:line 36
        // at Unity.Entities.World.CreateSystem(Type type)
        // at SystemsPOC.Plugin.RegisterSystems()
        // at SystemsPOC.Plugin.Load()
        // at Bloodstone.API.Reload.LoadPlugin(String path)



        // TODO: system registration needs to be deferred somehow
        //
        // [Error  :Bloodstone] Plugin SystemsPOC.Plugin threw an exception during initialization:
        // [Error  :Bloodstone] System.Exception: There is no Server world (yet). Did you install a server mod on the client?
        //   at Bloodstone.API.VWorld.get_Server()
        //   at SystemsPOC.Plugin.Load()
        //   at Bloodstone.API.Reload.LoadPlugin(String path)
    }

    public override bool Unload()
    {
        //UnregisterSystems();
        CommandRegistry.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }

    private void UnregisterSystems() {
        VWorld.Server.DestroySystem(mySystem);
        // TODO: system destruction needs to be deferred somehow. can't destroy a system while a system (e.g. one that handles messages/commands) is updating
    }

    // // Uncomment for example commmand or delete

    // /// <summary> 
    // /// Example VCF command that demonstrated default values and primitive types
    // /// Visit https://github.com/decaprime/VampireCommandFramework for more info 
    // /// </summary>
    // /// <remarks>
    // /// How you could call this command from chat:
    // ///
    // /// .systemspoc-example "some quoted string" 1 1.5
    // /// .systemspoc-example boop 21232
    // /// .systemspoc-example boop-boop
    // ///</remarks>
    // [Command("systemspoc-example", description: "Example command from systemspoc", adminOnly: true)]
    // public void ExampleCommand(ICommandContext ctx, string someString, int num = 5, float num2 = 1.5f)
    // { 
    //     ctx.Reply($"You passed in {someString} and {num} and {num2}");
    // }
}
