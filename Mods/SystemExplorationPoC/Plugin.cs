using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
using StableNameDotNet;
using Unity.Entities;
using VampireCommandFramework;
using VRisingMods.Core.Utilities;

namespace SystemExplorationPoC;

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

        // explore the world's systems
        // TrySomething();

        var world = WorldUtil.Game;
        var systemGroupTypes = FindSystemGroupTypes();
        var systemGroupInstances = new List<(Type, ComponentSystemGroup)>();
        var notFoundSystemGroupTypes = new List<Type>();
        foreach (var systemGroupType in systemGroupTypes)
        {
            var systemInstance = world.GetExistingSystemManaged(Il2CppType.From(systemGroupType));
            if (systemInstance is null)
            {
                notFoundSystemGroupTypes.Add(systemGroupType);
                continue;
            }
            var systemGroupInstance = systemInstance.Cast<ComponentSystemGroup>();

            // use a tuple to remember the original type
            // todo: or somehow cast it in here. not sure how to do it with a dynamic type though.
            systemGroupInstances.Add((systemGroupType, systemGroupInstance));
        }
        Log.LogInfo($"Found {systemGroupInstances.Count} system groups instances. The other {notFoundSystemGroupTypes.Count} are not being used by this world.");
        LogSystemGroups(systemGroupInstances);


        // todo: build a tree with groups and all the systems in them. ordered by update order if possible. note that groups can contain more groups.
    }

    public override bool Unload()
    {
        CommandRegistry.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }

    private void LogSystemGroups(IList<(Type, ComponentSystemGroup)> systemGroupInstances)
    {
        Log.LogInfo("[begin Listing system groups]================================================");
        foreach (var (systemGroupType, systemGroup) in systemGroupInstances)
        {
            var subsystems = systemGroup.GetAllSystems();
            Log.LogInfo($"{systemGroupType} contains {subsystems.Length} other systems");
            // todo: log more stuff
        }
        Log.LogInfo("[end Listing system groups]================================================");
    }

    private IList<Type> FindSystemGroupTypes()
    {
        var systemGroupTypes = new List<Type>();
        Assembly assembly = typeof(ProjectM.ReactToContestEventGroup).Assembly;
        foreach (var type in assembly.GetTypes())
        {
            if (type.IsSubclassOf(typeof(Unity.Entities.ComponentSystemGroup)))
            {
                systemGroupTypes.Add(type);
            }
        }
        return systemGroupTypes;
    }

    private void TrySomething()
    {
        var world = WorldUtil.Game;
        var system = world.GetExistingSystemManaged(Il2CppType.Of<ProjectM.ReactToContestEventGroup>());
        if (system is not null)
        {
            var systemGroup = system.Cast<ProjectM.ReactToContestEventGroup>();
            Log.LogInfo($"group contains {systemGroup.GetAllSystems().Length} systems");
        }
    }
    
}
