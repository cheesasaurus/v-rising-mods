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

namespace FrostDashFreezeFix.Patches;


// [HarmonyPatch(typeof(Spawn_DashSystem), nameof(Spawn_DashSystem.OnUpdate))]
public static class AsdfPatch
{
    public static void Postfix(Spawn_DashSystem __instance)
    {
        var i = 0;
        foreach (var query in __instance.EntityQueries)
        {
            LogUtil.LogInfo($"\nQuery#{i} -----------------------------\n");
            var events = query.ToEntityArray(Allocator.Temp);
            foreach (var entity in events)
            {
                DebugUtil.LogComponentTypes(entity);
                FreezeFixUtil.BuffWillBeSpawned(entity);
                //break;
            }
            i++;
        }

        FreezeFixUtil.CancelBadFreezeEvents();
    }

}

