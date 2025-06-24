using System;
using HookDOTS.API.Attributes;
using ProjectM;

namespace cheesasaurus.VRisingMods.EventScheduler;

public static class Patches
{
    public static event Action BeforeChatMessageSystemUpdates;

    [EcsSystemUpdatePrefix(typeof(RecursiveGroup), onlyWhenSystemRuns: false)]
    public static void ChatMessageSystem_Prefix()
    {
        BeforeChatMessageSystemUpdates?.Invoke();
    }

}
