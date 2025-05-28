using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using Il2CppInterop.Runtime;
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
        var systemGroupInstances = FindSystemGroupInstances(world, systemGroupTypes);

        var systemBaseTypes = FindComponentSystemBaseTypes();
        var systemBaseInstances = FindSystemBaseInstances(world, systemBaseTypes);

        var managedSystemsDict = new Dictionary<SystemHandle, (Type, ComponentSystemBase)>();
        foreach (var (systemGroupType, systemGroup) in systemGroupInstances)
        {
            // ComponentSystemGroup is a subclass of ComponentSystemBase, so add them first (more specific)
            managedSystemsDict.TryAdd(systemGroup.SystemHandle, (systemGroupType, systemGroup));
        }
        foreach (var (systemBaseType, systemBase) in systemBaseInstances)
        {
            // add all other instances of ComponentSystemBase whose intial types we're aware of
            managedSystemsDict.TryAdd(systemBase.SystemHandle, (systemBaseType, systemBase));
        }
        foreach (var systemInstance in world.Systems)
        {
            // there shouldn't be any other managed systems, but you never know ¯\_(ツ)_/¯
            // managedSystemsDict.TryAdd(systemInstance.SystemHandle, (systemInstance.GetType(), systemInstance));
        }

        var potentialUnmanagedSystemTypes = FindPotentialUnmanagedSystemTypes();
        var unmanagedSystemHandles = FindUnmanagedSystemHandles(world, potentialUnmanagedSystemTypes);
        var unmanagedSystemsDict = new Dictionary<SystemHandle, Type>();
        foreach (var (unmanagedSystemType, systemHandle) in unmanagedSystemHandles)
        {
            unmanagedSystemsDict.TryAdd(systemHandle, unmanagedSystemType);
        }

        LogSystemGroups(world, systemGroupInstances, managedSystemsDict, unmanagedSystemsDict);


        // todo: build a tree with groups and all the systems in them. ordered by update order if possible. note that groups can contain more groups.
    }

    public override bool Unload()
    {
        CommandRegistry.UnregisterAssembly();
        _harmony?.UnpatchSelf();
        return true;
    }

    private List<(Type, ComponentSystemGroup)> FindSystemGroupInstances(World world, IList<Type> systemGroupTypes)
    {
        var instances = new List<(Type, ComponentSystemGroup)>();
        var notFoundTypes = new List<Type>();
        foreach (var systemGroupType in systemGroupTypes)
        {
            try
            {
                var systemInstance = world.GetExistingSystemManaged(Il2CppType.From(systemGroupType));
                if (systemInstance is null)
                {
                    notFoundTypes.Add(systemGroupType);
                    continue;
                }
                var systemGroupInstance = systemInstance.Cast<ComponentSystemGroup>();

                // use a tuple to remember the original type
                // todo: or somehow cast it in here. not sure how to do it with a dynamic type though.
                instances.Add((systemGroupType, systemGroupInstance));
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Failure finding instance of {systemGroupType}: {ex}");
            }
        }
        Log.LogInfo($"Found {instances.Count} ComponentSystemGroup instances. The other {notFoundTypes.Count} are not being used by this world.");
        return instances;
    }

    private List<(Type, ComponentSystemBase)> FindSystemBaseInstances(World world, IList<Type> systemBaseTypes)
    {
        var instances = new List<(Type, ComponentSystemBase)>();
        var notFoundTypes = new List<Type>();
        foreach (var systemBaseType in systemBaseTypes)
        {
            try
            {
                var systemInstance = world.GetExistingSystemManaged(Il2CppType.From(systemBaseType));
                if (systemInstance is null)
                {
                    notFoundTypes.Add(systemBaseType);
                    continue;
                }
                var systemBaseInstance = systemInstance.Cast<ComponentSystemBase>();

                // use a tuple to remember the original type
                // todo: or somehow cast it in here. not sure how to do it with a dynamic type though.
                instances.Add((systemBaseType, systemBaseInstance));
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Error finding instance of {systemBaseType}: {ex}");
            }
        }
        Log.LogInfo($"Found {instances.Count} system base instances. The other {notFoundTypes.Count} are not being used by this world.");
        return instances;
    }

    private List<(Type, SystemHandle)> FindUnmanagedSystemHandles(World world, IList<Type> systemTypes)
    {
        var instances = new List<(Type, SystemHandle)>();
        var notFoundTypes = new List<Type>();
        foreach (var systemType in systemTypes)
        {
            try
            {
                var systemHandle = world.GetExistingSystem(Il2CppType.From(systemType));
                if (systemHandle.Equals(SystemHandle.Null))
                {
                    notFoundTypes.Add(systemType);
                    continue;
                }
                instances.Add((systemType, systemHandle));
            }
            catch (Exception ex)
            {
                Log.LogWarning($"Error finding instance of {systemType}: {ex}");
            }
        }
        Log.LogInfo($"Found {instances.Count} ISystem instances. The other {notFoundTypes.Count} are not being used by this world.");
        return instances;
    }

    private void LogSystemGroups(
        World world,
        IList<(Type, ComponentSystemGroup)> systemGroupInstances,
        Dictionary<SystemHandle, (Type, ComponentSystemBase)> managedSystemsDict,
        Dictionary<SystemHandle, Type> unmanagedSystemsDict
    )
    {
        Log.LogInfo($"[begin Listing system groups in world {world.Name}]================================================");
        foreach (var (systemGroupType, systemGroup) in systemGroupInstances)
        {
            var orderedSubsystems = systemGroup.GetAllSystems();
            Log.LogInfo($"{systemGroupType} contains {orderedSubsystems.Length} other systems");

            foreach (var subsystemHandle in orderedSubsystems)
            {
                if (managedSystemsDict.ContainsKey(subsystemHandle))
                {
                    var (bestKnownType, subsystem) = managedSystemsDict[subsystemHandle];
                    Log.LogInfo($"  {bestKnownType}");
                }
                else if (unmanagedSystemsDict.ContainsKey(subsystemHandle))
                {
                    var knownType = unmanagedSystemsDict[subsystemHandle];
                    Log.LogInfo($"  {knownType} (unmanaged)");
                }
                else
                {
                    Log.LogInfo($"  unknown system");
                }
            }

            // todo: log more stuff
        }
        Log.LogInfo("[end Listing system groups]================================================");
    }

    private IList<Type> FindSystemGroupTypes()
    {
        var systemGroupTypes = new List<Type>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assemblyCount = assemblies.Length;
        var counter = 0;
        foreach (var assembly in assemblies)
        {
            Log.LogInfo($"scanning assembly {++counter} of {assemblyCount}");
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    try
                    {
                        if (type.IsSubclassOf(typeof(Unity.Entities.ComponentSystemGroup)))
                        {
                            systemGroupTypes.Add(type);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogWarning(ex);
                    }

                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex);
            }

        }
        return systemGroupTypes;
    }

    

    // todo: we should scan all assemblies once, but this is a proof-of-concept and i'm too lazy to refactor
    private IList<Type> FindComponentSystemBaseTypes()
    {
        var systemBaseTypes = new List<Type>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assemblyCount = assemblies.Length;
        var counter = 0;
        foreach (var assembly in assemblies)
        {
            Log.LogInfo($"scanning assembly {++counter} of {assemblyCount}");
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    try
                    {
                        if (type.IsSubclassOf(typeof(Unity.Entities.ComponentSystemBase)))
                        {
                            systemBaseTypes.Add(type);
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.LogWarning(ex);
                    }

                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex);
            }

        }
        return systemBaseTypes;
    }

    // todo: we should scan all assemblies once, but this is a proof-of-concept and i'm too lazy to refactor
    private IList<Type> FindPotentialUnmanagedSystemTypes()
    {
        var iSystemTypes = new List<Type>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assemblyCount = assemblies.Length;
        var counter = 0;
        foreach (var assembly in assemblies)
        {
            Log.LogInfo($"scanning assembly {++counter} of {assemblyCount}");
            try
            {
                foreach (var type in assembly.GetTypes())
                {
                    try
                    {
                        // note: the generated interops don't mark structs with ISystem which is probably a bug in the Il2CppInterop library
                        // (the metadata does contain that information during generation)
                        /*
                        if (type.IsAssignableFrom(typeof(Unity.Entities.ISystem)))
                        {
                            iSystemTypes.Add(type);
                        }
                        */

                        if (!type.IsValueType)
                        {
                            continue;
                        }
                        if (type.GetMethod("OnCreate") is null)
                        {
                            continue;
                        }
                        if (type.GetMethod("OnDestroy") is null)
                        {
                            continue;
                        }
                        if (type.GetMethod("OnUpdate") is null)
                        {
                            continue;
                        }
                        iSystemTypes.Add(type);
                        
                    }
                    catch (Exception ex)
                    {
                        Log.LogWarning(ex);
                    }

                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex);
            }

        }
        return iSystemTypes;
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
