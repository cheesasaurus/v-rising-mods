using System;
using System.Collections.Generic;
using System.Linq;
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
        LogUtil.LogInfo($"building hierarchy for world: {world.Name}");
        var nodes = FindSystems(world, out var counts);
        var groupNodes = nodes.Values.Where(node => node.Category is EcsSystemCategory.Group);

        foreach (var groupNode in groupNodes)
        {
            try
            {
                var group = (ComponentSystemGroup)groupNode.Instance;
                var orderedSubsystems = group.GetAllSystems();
                foreach (var subsystemHandle in orderedSubsystems)
                {
                    var childNode = nodes[subsystemHandle];
                    groupNode.ChildrenOrderedForUpdate.Add(childNode);
                    if (childNode.Parents.Count > 0)
                    {
                        Log.LogError($"Uh oh, a system belongs to multiple groups. This should not happen: {childNode.Type}");
                    }
                    childNode.Parents.Add(groupNode);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogDebug($"{groupNode.Type.FullName} ({groupNode.Category})");
                LogUtil.LogWarning(ex);
            } 
        }

        var rootNodes = nodes.Values.Where(node => node.Parents.Count is 0);

        return new EcsSystemHierarchy
        {
            World = world,
            Counts = counts,
            KnownUnknowns = new KnownUnknowns(),
            RootNodesUnordered = rootNodes.ToList(),
        };
    }

    private Dictionary<SystemHandle, EcsSystemTreeNode> FindSystems(World world, out EcsSystemCounts counts)
    {
        var nodes = new Dictionary<SystemHandle, EcsSystemTreeNode>();
        counts = new EcsSystemCounts();

        var systemTypeIndices = TypeManager.GetSystemTypeIndices();
        foreach (var systemTypeIndex in systemTypeIndices)
        {
            var systemHandle = world.GetExistingSystem(systemTypeIndex);
            if (!world.Unmanaged.IsSystemValid(systemHandle))
            {
                counts.NotUsed++;
                continue;
            }

            var systemType = world.Unmanaged.GetTypeOfSystem(systemHandle);
            var category = CategorizeSystem(systemTypeIndex);
            Log.LogInfo($"  {systemType.FullName} ({category})");

            var node = new EcsSystemTreeNode(
                category: category,
                systemHandle: systemHandle,
                type: systemType,
                instance: world.GetExistingSystemInternal(systemTypeIndex)
            );
            nodes.Add(systemHandle, node);

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
        }

        return nodes;
    }

    private EcsSystemCategory CategorizeSystem(SystemTypeIndex systemTypeIndex)
    {
        return systemTypeIndex.IsGroup ? EcsSystemCategory.Group : systemTypeIndex.IsManaged ? EcsSystemCategory.Base : EcsSystemCategory.Unmanaged;
    }

}
