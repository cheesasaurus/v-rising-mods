using System.IO;
using BepInEx.Logging;
using cheesasaurus.VRisingMods.SystemsDumper.Dumpers;
using cheesasaurus.VRisingMods.SystemsDumper.Services;
using Unity.Entities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Patches;

public class DumpService
{
    ManualLogSource Log;
    EcsSystemHierarchyService EcsSystemHierarchyService;
    EcsSystemWithEntityQueriesService EcsSystemWithEntityQueriesService;

    public DumpService(ManualLogSource log, EcsSystemHierarchyService ecsSystemHierarchyService, EcsSystemWithEntityQueriesService ecsSystemWithEntityQueriesService)
    {
        Log = log;
        EcsSystemHierarchyService = ecsSystemHierarchyService;
        EcsSystemWithEntityQueriesService = ecsSystemWithEntityQueriesService;
    }

    public string DumpSystemsUpdateTrees()
    {
        var dir = "Dump/Systems";
        var dumper = new EcsSystemUpdateTreeDumper(spacesPerIndent: 4);
        foreach (var world in World.s_AllWorlds)
        {
            var worldDir = $"{dir}/{world.Name}";
            Directory.CreateDirectory(worldDir);
            var systemHierarchy = EcsSystemHierarchyService.BuildSystemHiearchyForWorld(world);
            File.WriteAllText($"{worldDir}/UpdateTree.txt", dumper.CreateDumpString(systemHierarchy));
        }
        var pattern = $"{dir}/<WorldName>/UpdateTree.txt";
        Log.LogMessage($"Dumped system update tree files as {pattern}");
        return pattern;
    }

    public string DumpSystemsEntityQueries()
    {
        var dir = "Dump/Systems";
        var dumper = new EntityQueriesDumper(spacesPerIndent: 4);
        foreach (var world in World.s_AllWorlds)
        {
            var worldDir = $"{dir}/{world.Name}";
            Directory.CreateDirectory(worldDir);
            var systemModels = EcsSystemWithEntityQueriesService.FindSystemsWithEntityQueries(world);
            File.WriteAllText($"{worldDir}/EntityQueries.txt", dumper.ListAllQueries(world, systemModels));
        }
        var pattern = $"{dir}/<WorldName>/EntityQueries.txt";
        Log.LogMessage($"Dumped system entity query files as {pattern}");
        return pattern;
    }

}