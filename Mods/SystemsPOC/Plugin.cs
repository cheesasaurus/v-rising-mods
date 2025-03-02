using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace SystemsPOC;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("gg.deca.VampireCommandFramework")]
public class Plugin : BasePlugin
{
    Harmony _harmony;

    private static MySystem _mySystem;
    private static bool _registeredSystems;
    private static InitWhenReadyBehaviour _initBehaviour;


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

        // defer some things until world is ready
        _initBehaviour = AddComponent<InitWhenReadyBehaviour>();
    }

    public override bool Unload()
    {
        if (_initBehaviour != null) {
            UnityEngine.Object.Destroy(_initBehaviour);
        }
        UnregisterSystems();
        CommandRegistry.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }

    public static void RegisterSystems()
    {
        RegisterSystems(WorldUtil.Game);
    }

    public static void RegisterSystems(World world)
    {
        LogUtil.LogInfo($"Adding own systems to world: {world.Name}");
        if (!ClassInjector.IsTypeRegisteredInIl2Cpp<MySystem>())
        {
            // todo: need some way to unregister the type when unloading. if the plugin is hot-reloaded, the type lingers and is incorrect (refers to the old assembly).
            // RegisterTypeInIl2Cpp throws an error if trying to re-register something with the same FullName.
            // We might be able to remove the type from ClassInjector.InjectedTypes, but it's private. will require some finagling
            ClassInjector.RegisterTypeInIl2Cpp<MySystem>();
        }

        LogUtil.LogInfo($"  There were previously {world.Unmanaged.GetAllSystems(Allocator.Temp).Length} systems in {world.Name}");
        _mySystem = world.CreateSystemManaged<MySystem>();

        var systemType = Il2CppType.Of<MySystem>();
        var systemTypeIndex = TypeManager.GetSystemTypeIndex(systemType);
        LogUtil.LogInfo($"  Created {_mySystem.GetType()} with SystemTypeIndex.Index {systemTypeIndex.Index}");
        LogUtil.LogInfo($"  {_mySystem.GetType()}.Enabled: {_mySystem.Enabled}");

        var systemGroup = world.GetExistingSystemManaged<UpdateGroup>();
        if (systemGroup != null)
        {
            LogUtil.LogInfo($"    There were previously {systemGroup.GetAllSystems().Length} systems in {systemGroup.GetType()}");
            LogUtil.LogInfo($"    Adding {_mySystem.GetType()} to {systemGroup.GetType()}");
            systemGroup.AddSystemToUpdateList(_mySystem);
            LogUtil.LogInfo($"    There are now {systemGroup.GetAllSystems().Length} systems in {systemGroup.GetType()}");
        }

        LogUtil.LogInfo($"  There are now {world.Unmanaged.GetAllSystems(Allocator.Temp).Length} systems in {world.Name}");

        _registeredSystems = true;

        // Log:

        // [Info   :SystemsPOC] Adding own systems to world: Server
        // [Info   :SystemsPOC] Adding own systems to world: Server
        // [Info   :Il2CppInterop] Registered mono type SystemsPOC.MySystem in il2cpp domain
        // [Info   :SystemsPOC]   There were previously 1066 systems in Server
        // [Info   :SystemsPOC] MySystem.OnCreate called
        // [Info   :SystemsPOC]   Created SystemsPOC.MySystem with SystemTypeIndex.Index 2069
        // [Info   :SystemsPOC]   SystemsPOC.MySystem.Enabled: True
        // [Info   :SystemsPOC]     There were previously 330 systems in ProjectM.UpdateGroup
        // [Info   :SystemsPOC]     Adding SystemsPOC.MySystem to ProjectM.UpdateGroup
        // [Info   :SystemsPOC]     There are now 331 systems in ProjectM.UpdateGroup
        // [Info   :SystemsPOC]   There are now 1067 systems in Server
        // [Error  :     Unity] AssertionException: Assertion failure. Values are not equal.
        // Expected: 331 == 330
        // [Error  :     Unity] AssertionException: Assertion failure. Values are not equal.
        // Expected: 331 == 330
        // [Error  :     Unity] AssertionException: Assertion failure. Values are not equal.
        // Expected: 331 == 330
        // [Error  :     Unity] AssertionException: Assertion failure. Values are not equal.
        // Expected: 331 == 330
        // [Error  :     Unity] AssertionException: Assertion failure. Values are not equal.
        // Expected: 331 == 330

        // ... and then endless assertion spam about "Expected: 331 == 330".
        // There seems to be some check intentionally preventing the exact thing we want to do
        // (register custom systems from hot-reloaded plugins)
    }

    private static void UnregisterSystems()
    {
        if (!_registeredSystems) {
            return;
        }
        _registeredSystems = false;
        WorldUtil.Game.DestroySystem(_mySystem.SystemHandle);
        // TODO: system destruction needs to be deferred somehow. can't destroy a system while a system (e.g. one that handles messages/commands) is updating
    }

    private class InitWhenReadyBehaviour : UnityEngine.MonoBehaviour
    {
        private bool _initialized = false;

        private void Update()
        {
            if (_initialized || !WorldUtil.IsGameWorldCreated())
            {
                return;
            }
            _initialized = true;
            RegisterSystems();
        }
    }

}
