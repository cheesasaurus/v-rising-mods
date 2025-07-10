using System.IO;
using BepInEx.Logging;
using cheesasaurus.VRisingMods.SystemsDumper.CodeGeneration;
using cheesasaurus.VRisingMods.SystemsDumper.Dumpers;
using cheesasaurus.VRisingMods.SystemsDumper.Services;
using Unity.Entities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Patches;

public class DumpService
{
    ManualLogSource Log;
    EcsSystemHierarchyService EcsSystemHierarchyService;
    EcsSystemMetadataService EcsSystemWithEntityQueriesService;

    public DumpService(ManualLogSource log, EcsSystemHierarchyService ecsSystemHierarchyService, EcsSystemMetadataService ecsSystemWithEntityQueriesService)
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
            var systemModels = EcsSystemWithEntityQueriesService.CollectSystemMetadata(world);
            File.WriteAllText($"{worldDir}/EntityQueries.txt", dumper.ListAllQueries(world, systemModels));
        }

        var pattern = $"{dir}/<WorldName>/EntityQueries.txt";
        Log.LogMessage($"Dumped system entity query files as {pattern}");
        return pattern;
    }

    public string DumpSystemsCodeGen()
    {
        var dir = "Dump/Systems";
        var systemCodeGenFactory = new EcsSystemCodeGenerator.Factory(spacesPerIndent: 4, newLine: "\n");

        foreach (var world in World.s_AllWorlds)
        {
            var codeDir = $"{dir}/{world.Name}/Code";
            Directory.CreateDirectory(codeDir);
            var systemMetadatas = EcsSystemWithEntityQueriesService.CollectSystemMetadata(world);

            foreach (var systemMetadata in systemMetadatas)
            {
                var systemCodeGenerator = systemCodeGenFactory.CodeGenerator(world, systemMetadata);
                File.WriteAllText(
                    path: $"{codeDir}/{systemMetadata.Type.FullName}.cs",
                    contents: systemCodeGenerator.CreateCSharpFileContents()
                );
            }
        }

        var pattern = $"{dir}/<WorldName>/Code/<SystemFullName>.cs";
        Log.LogMessage($"Generated code snippet files as {pattern}");
        return pattern;
    }

}