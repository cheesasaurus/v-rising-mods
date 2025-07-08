using System.IO;
using Unity.Entities;
using VampireCommandFramework;

namespace cheesasaurus.VRisingMods.SystemsDumper.Commands
{
    [CommandGroup("DumpSystems", "ds")]
    internal class DumpCommands
    {

        [Command("UpdateTree", "ut", description: "Dumps ECS system update hierarchies to files (per world)", adminOnly: true)]
        public static void DumpSystems(ChatCommandContext ctx)
        {
            var dir = "Dump/Systems/UpdateTree/";
            Directory.CreateDirectory(dir);
            var dumper = new EcsSystemDumper(spacesPerIndent: 4);
            foreach (var world in World.s_AllWorlds)
            {
                var systemHierarchy = Core.EcsSystemHierarchyService.BuildSystemHiearchyForWorld(world);
                File.WriteAllText($"{dir}/{world.Name}.txt", dumper.CreateDumpString(systemHierarchy));
            }
            ctx.Reply($"Dumped system hierarchy files to {dir} folder");
        }
        
    }
}
