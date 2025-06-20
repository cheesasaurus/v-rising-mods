using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using ProjectM.Network;
using Stunlock.Core;
using System.Linq;
using VRisingMods.Core.Utilities;
using ProjectM.Gameplay;
using ProjectM.Gameplay.Systems;

namespace FrostDashFreezeFix.Patches;


//[HarmonyPatch(typeof(AbilitySpawnSystem), nameof(AbilitySpawnSystem.OnUpdate))]
public static class AsdfPatch
{
    public static void Prefix(AbilitySpawnSystem __instance)
    {
        LogUtil.LogInfo($"{FreezeFixUtil.RecursiveTickStamp} ASDF patch");

        var i = 0;
        foreach (var query in __instance.EntityQueries)
        {
            LogUtil.LogInfo($"\nQuery#{i} -----------------------------\n");
            var events = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in events)
            {
                DebugUtil.LogComponentTypes(entity);
                //FreezeFixUtil.BuffWillBeSpawned(entity);
                FreezeFixUtil.LogBuffThings(entity);
                //break;
            }
            i++;
        }

        FreezeFixUtil.CancelBadFreezeEvents();
    }

}

