using BepInEx.Logging;
using cheesasaurus.VRisingMods.SystemsDumper.Patches;
using cheesasaurus.VRisingMods.SystemsDumper.Services;

namespace cheesasaurus.VRisingMods.SystemsDumper;

public static class Core
{
    public static ManualLogSource Log { get; } = Plugin.LogInstance;
    public static EcsSystemHierarchyService EcsSystemHierarchyService { get; } = new(Log);
    public static DumpService DumpService { get; } = new(EcsSystemHierarchyService, Log);
}