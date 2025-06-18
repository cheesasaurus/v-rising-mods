using HarmonyLib;
using HookDOTS.API.Attributes;
using ProjectM;
using Unity.Entities;

namespace FrostDashFreezeFix.Patches;


public unsafe class MiscPatches
{
    [EcsSystemUpdatePrefix(typeof(RecursiveGroup), onlyWhenSystemRuns: false)]
    public static void UpdateTickCount()
    {
        FreezeFixUtil.NewTickStarted();

    }

    //[EcsSystemUpdatePrefix(typeof(RecursiveGroup))]
    //public static void InitInflictionMarkers
    //{
    //    
    //}

}