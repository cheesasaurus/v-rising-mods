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


//[HarmonyPatch(typeof(Apply_BuffModificationsSystem_Server), nameof(Apply_BuffModificationsSystem_Server.OnUpdate))]
public static class AsdfPatch
{
    public static bool Prefix(Apply_BuffModificationsSystem_Server __instance)
    {
        return false;
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

