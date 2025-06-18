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


//[HarmonyPatch(typeof(Apply_BuffModificationsSystem_Server), nameof(Apply_BuffModificationsSystem_Server.OnUpdate))]
public static class ApplyBuffModsPatch
{
    public static void Prefix(Apply_BuffModificationsSystem_Server __instance)
    {
        // todo: pick this beast apart; its got 8 queries and a bunch of jobs
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
