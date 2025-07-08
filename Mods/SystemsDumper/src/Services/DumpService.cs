using System.IO;
using BepInEx.Logging;
using cheesasaurus.VRisingMods.SystemsDumper.Services;
using Unity.Entities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Patches;

public class DumpService
{
    ManualLogSource Log;
    EcsSystemHierarchyService EcsSystemHierarchyService;

    public DumpService(EcsSystemHierarchyService ecsSystemHierarchyService, ManualLogSource log)
    {
        Log = log;
        EcsSystemHierarchyService = ecsSystemHierarchyService;
    }

    public string DumpSystemsUpdateTrees()
    {
        var dir = "Dump/Systems/UpdateTree/";
        Directory.CreateDirectory(dir);
        var dumper = new EcsSystemDumper(spacesPerIndent: 4);
        foreach (var world in World.s_AllWorlds)
        {
            var systemHierarchy = EcsSystemHierarchyService.BuildSystemHiearchyForWorld(world);
            File.WriteAllText($"{dir}/{world.Name}.txt", dumper.CreateDumpString(systemHierarchy));
        }
        Log.LogMessage($"Dumped system hierarchy files to folder {dir}");
        return dir;
    }

}