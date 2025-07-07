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
        var counts = new EcsSystemCounts();

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

            var category = systemTypeIndex.IsGroup ? EcsSystemCategory.Group : systemTypeIndex.IsManaged ? EcsSystemCategory.Managed : EcsSystemCategory.Unmanaged;
            switch (category)
            {
                case EcsSystemCategory.Group:
                    counts.Group++;
                    break;
                case EcsSystemCategory.Managed:
                    counts.Managed++;
                    break;
                case EcsSystemCategory.Unmanaged:
                    counts.Unmanaged++;
                    break;
            }


            Log.LogInfo($"  {systemType.FullName} ({category})");

            //LogUtil.LogInfo(TypeManager.GetSystemName(systemTypeIndex));
        }

        return new EcsSystemHierarchy
        {
            World = world,
            Counts = counts,
            KnownUnknowns = new KnownUnknowns(),
            RootNodesUnordered = new List<EcsSystemTreeNode>(),
        };

        //throw new NotImplementedException();
        // todo: implement
    }

}
