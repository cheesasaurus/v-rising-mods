using System.IO;
using Unity.Entities;
using VampireCommandFramework;

namespace cheesasaurus.VRisingMods.SystemsDumper.Commands
{
    [CommandGroup("dump")]
    internal class DumpCommands
    {

        [Command("systems", "s", description: "Dumps ECS system hierarchies to files (per world)", adminOnly: true)]
        public static void DumpSystems(ChatCommandContext ctx)
        {
            var dir = "dump/systems/";
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
