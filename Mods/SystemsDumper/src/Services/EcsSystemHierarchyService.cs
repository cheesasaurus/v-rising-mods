using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Unity.Entities;
using Unity.Physics.Systems;
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
                        LogUtil.LogWarning($"A Group's child system does not exist within the world. Group: {groupNode.Type.FullName} ({groupNode.Category})");
                        counts.Unknown++;
                        knownUnknowns.SystemNotFoundInWorld.Add(subsystemHandle);

                        // BuildStaticPhysicsWorld ?
                        // EndFixedStepSimulationEntityCommandBufferSystem ?

                        // There are only 2 systems where this happens.
                        // Both are in Unity.Entities.FixedStepSimulationSystemGroup.
                        // One of them is likely to be Unity.Physics.Systems.BuildStaticPhysicsWorld (ISystem)
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
                        Log.LogError($"Uh oh, a system belongs to multiple groups. This should not happen: {childNode.Type}");
                    }
                    childNode.Parents.Add(groupNode);
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogWarning($"{groupNode.Type.FullName} ({groupNode.Category})");
                LogUtil.LogWarning(ex);
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

        var systemTypeIndices = TypeManager.GetSystemTypeIndices();
        foreach (var systemTypeIndex in systemTypeIndices)
        {
            var systemHandle = world.GetExistingSystem(systemTypeIndex);
            if (systemHandle.Equals(SystemHandle.Null))
            {
                counts.NotUsed++;
                continue;
            }
            if (!world.Unmanaged.IsSystemValid(systemHandle))
            {
                //counts.NotUsed++;
                //continue;
            }

            var systemType = world.Unmanaged.GetTypeOfSystem(systemHandle);
            var category = CategorizeSystem(systemTypeIndex);

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
