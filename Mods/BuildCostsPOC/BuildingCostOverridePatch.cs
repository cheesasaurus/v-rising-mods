using System.Collections.Generic;
using HarmonyLib;
using ProjectM.CastleBuilding.Placement;
using Stunlock.Core;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.BuildCostsPOC;

/// <remarks>
/// from kil.jaeden on discord
/// 
/// placing a palisade door costs an extra 1 power core
/// </remarks>
[HarmonyPatch]
public class BuildingCostOverridePatch
{
    private static readonly Dictionary<PrefabGUID, List<(PrefabGUID, int)>> CustomCostMap = new()
    {
        [new PrefabGUID(-1259825508)] = new List<(PrefabGUID, int)>
        {
            (new PrefabGUID(-1190647720), 1)
        }
    };

    [HarmonyPatch(typeof(GetPlacementResourcesResult),
        nameof(GetPlacementResourcesResult.AddResourceRequirements))]
    [HarmonyPrefix]
    public static bool GetPlacementResourcesResult_Prefix(
        EntityManager entityManager,
        Entity entity,
        NativeParallelHashMap<PrefabGUID, int> result)
    {
        LogUtil.LogInfo($"Adding resource requirements for {entity}.");
        if (!entityManager.HasComponent<PrefabGUID>(entity))
            return true;

        var buildingGUID = entityManager.GetComponentData<PrefabGUID>(entity);

        if (!CustomCostMap.TryGetValue(buildingGUID, out var customCosts))
            return true;

        foreach (var (resourceGUID, quantity) in customCosts)
        {
            int current = result.TryGetValue(resourceGUID, out var existing) ? existing : 0;
            result[resourceGUID] = current + quantity;
        }

        return false;
    }
}