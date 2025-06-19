using HarmonyLib;
using ProjectM;
using Unity.Collections;
using Unity.Entities;
using System.Collections.Generic;
using ProjectM.Network;
using Stunlock.Core;
using System.Linq;

namespace FrostDashFreezeFix.Patches;


//[HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
public static class BuffSpawnPatch
{
    public static void Prefix(BuffSystem_Spawn_Server __instance)
    {
        var events = __instance._Query.ToEntityArray(Allocator.Temp);
        foreach (var entity in events)
        {
            FreezeFixUtil.BuffWillBeSpawned(entity);
        }

        FreezeFixUtil.CancelBadFreezeEvents();
    }

}
