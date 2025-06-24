using System;
using HarmonyLib;
using HookDOTS.API.Attributes;
using ProjectM;

namespace cheesasaurus.VRisingMods.EventScheduler;

[HarmonyPatch]
public static class Hooks
{
    public static event Action BeforeChatMessageSystemUpdates;
    public static event Action BeforeWorldSave;

    [EcsSystemUpdatePrefix(typeof(RecursiveGroup), onlyWhenSystemRuns: false)]
    public static void ChatMessageSystem_Prefix()
    {
        BeforeChatMessageSystemUpdates?.Invoke();
    }

    [HarmonyPatch(typeof(TriggerPersistenceSaveSystem), nameof(TriggerPersistenceSaveSystem.TriggerSave))]
    [HarmonyPrefix]
    public static void TriggerSave_Prefix()
    {
        BeforeWorldSave?.Invoke();
    }

}
