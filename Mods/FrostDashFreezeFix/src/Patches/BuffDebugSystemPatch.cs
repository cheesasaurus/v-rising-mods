using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using ProjectM.Network;
using Stunlock.Core;
using System.Linq;
using VRisingMods.Core.Utilities;

namespace FrostDashFreezeFix.Patches;


//[HarmonyPatch(typeof(BuffDebugSystem), nameof(BuffDebugSystem.OnUpdate))]
public static class BuffDebugSystemPatch
{
    public static void Prefix(BuffDebugSystem __instance)
    {
        var events = __instance.__query_401358787_0.ToEntityArray(Allocator.Temp);
        foreach (var entity in events)
        {
            DebugUtil.LogComponentTypes(entity);
            FreezeFixUtil.BuffWillBeSpawned(entity);
            //break;
        }

        FreezeFixUtil.CancelBadFreezeEvents();        
    }

}

