using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BepInEx.Logging;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Services;

// responsible for building a systems update hierarchy for any given world
public class EcsSystemHierarchyService
{
    private ManualLogSource Log;

    public EcsSystemHierarchyService(ManualLogSource log)
    {
        Log = log;
    }

    public EcsSystemHierarchy BuildSystemHiearchyForWorld(World world)
    {
        Log.LogInfo($"building hierarchy for world: {world.Name}");
        var knownUnknowns = new KnownUnknowns();
        var nodes = FindSystems(world, out var counts);
        var groupNodes = nodes.Values.Where(node => node.Category is EcsSystemCategory.Group);

        foreach (var groupNode in groupNodes)
        {
            try
            {
                var group = groupNode.Instance.Cast<ComponentSystemGroup>();
                var orderedSubsystems = group.GetAllSystems();
                foreach (var subsystemHandle in orderedSubsystems)
                {
                    if (!nodes.TryGetValue(subsystemHandle, out var childNode))
                    {
                        Log.LogWarning($"A Group's child system does not exist within the world. Group: {groupNode.Type.FullName} ({groupNode.Category})");
                        counts.Unknown++;
                        knownUnknowns.SystemNotFoundInWorld.Add(subsystemHandle);

                        childNode = new EcsSystemTreeNode(
                            category: EcsSystemCategory.Unknown,
                            systemHandle: subsystemHandle,
                            type: null,
                            instance: null
                        );
                    }

                    groupNode.ChildrenOrderedForUpdate.Add(childNode);
                    if (childNode.Parents.Count > 0)
                    {
                        Log.LogWarning($"Uh oh, a system belongs to multiple groups. This should not happen: {childNode.Type}");
                    }
                    childNode.Parents.Add(groupNode);
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning($"{groupNode.Type.FullName} ({groupNode.Category})");
                Log.LogWarning(ex);
            }
        }

        var rootNodes = nodes.Values.Where(node => node.Parents.Count is 0);

        return new EcsSystemHierarchy
        {
            World = world,
            Counts = counts,
            KnownUnknowns = knownUnknowns,
            RootNodesUnordered = rootNodes.ToList(),
        };
    }

    private Dictionary<SystemHandle, EcsSystemTreeNode> FindSystems(World world, out EcsSystemCounts counts)
    {
        var nodes = new Dictionary<SystemHandle, EcsSystemTreeNode>();
        counts = new EcsSystemCounts();

        var systemTypeIndices = TypeManager.GetSystemTypeIndices(WorldSystemFilterFlags.All, 0);
        foreach (var systemTypeIndex in systemTypeIndices)
        {
            var systemHandle = world.GetExistingSystem(systemTypeIndex);
            if (!world.Unmanaged.IsSystemValid(systemHandle))
            {
                counts.NotUsed++;
                continue;
            }

            var node = BuildNodeAndIncrementAppropriateCount(world, systemTypeIndex, counts);
            nodes.Add(systemHandle, node);
        }

        // TypeManager.GetSystemTypeIndices misses a few things somehow.
        // Do a different scan to discover more systems.
        foreach (var systemHandle in world.Unmanaged.GetAllSystems(Allocator.Temp))
        {
            if (nodes.ContainsKey(systemHandle))
            {
                continue;
            }

            if (TryGetSystemTypeIndex_ForMissedSystem(world, systemHandle, out var systemTypeIndex))
            {
                var node = BuildNodeAndIncrementAppropriateCount(world, systemTypeIndex, counts);
                nodes.Add(systemHandle, node);
            }
            else
            {
                LogUtil.LogWarning($"Failed to get SystemTypeIndex for a SystemHandle in world {world.Name}");
            }
        }

        return nodes;
    }

    private EcsSystemTreeNode BuildNodeAndIncrementAppropriateCount(World world, SystemTypeIndex systemTypeIndex, EcsSystemCounts counts)
    {
        var systemHandle = world.GetExistingSystem(systemTypeIndex);
        var systemType = world.Unmanaged.GetTypeOfSystem(systemHandle);
        var category = CategorizeSystem(systemTypeIndex);

        var node = new EcsSystemTreeNode(
            category: category,
            systemHandle: systemHandle,
            type: systemType,
            instance: world.GetExistingSystemInternal(systemTypeIndex)
        );

        switch (category)
        {
            case EcsSystemCategory.Group:
                counts.Group++;
                break;
            case EcsSystemCategory.Base:
                counts.Base++;
                break;
            case EcsSystemCategory.Unmanaged:
                counts.Unmanaged++;
                break;
        }

        return node;
    }

    private EcsSystemCategory CategorizeSystem(SystemTypeIndex systemTypeIndex)
    {
        return systemTypeIndex.IsGroup ? EcsSystemCategory.Group : systemTypeIndex.IsManaged ? EcsSystemCategory.Base : EcsSystemCategory.Unmanaged;
    }

    private bool TryGetSystemTypeIndex_ForMissedSystem(World world, SystemHandle systemHandle, out SystemTypeIndex systemTypeIndex, bool logDebug = true)
    {
        systemTypeIndex = SystemTypeIndex.Null;

        if (systemHandle.Equals(SystemHandle.Null))
        {
            return false;
        }
        if (!world.Unmanaged.IsSystemValid(systemHandle))
        {
            return false;
        }
        
        var systemType = world.Unmanaged.GetTypeOfSystem(systemHandle);
        if (systemType is null)
        {
            return false;
        }

        systemTypeIndex = TypeManager.GetSystemTypeIndex(systemType);

        if (logDebug)
        {
            var existsInList = false;
            foreach (var otherSystemTypeIndex in TypeManager.GetSystemTypeIndices())
            {
                if (systemTypeIndex.Equals(otherSystemTypeIndex))
                {
                    existsInList = true;
                    break;
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Found missed system {systemType.FullName}.");
            sb.AppendLine($"  SystemTypeIndex.Index: {systemTypeIndex.Index} (retrieved from TypeManager.GetSystemTypeIndex)");
            sb.Append($"  Is SystemTypeIndex in TypeManager.GetSystemTypeIndices: {existsInList}");
            if (!existsInList)
            {
                sb.Append($" (This might be a bug in the TypeManager)");
            }
            Log.LogDebug(sb.ToString());
        }

        return true;
    }

}
