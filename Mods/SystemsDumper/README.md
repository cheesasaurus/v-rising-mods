# SystemsDumper

SystemsDumper is a tool for mod developers.

It dumps information about ECS Systems. For use with both client and server applications.


## Installation

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/).
- Install [VampireCommandFramework](https://thunderstore.io/c/v-rising/p/deca/VampireCommandFramework/).
- Extract `SystemsDumper.dll` into `(VRising folder)/BepInEx/plugins`.


## Chat Commands

Commands work on both client and server applications. (client dumps are dumped locally, server dumps are dumped to the server). AdminAuth required.

- `.DumpSystems UpdateTree`
  - Dumps ECS system update hierarchies to files. One file per World. [Sample](https://github.com/cheesasaurus/v-rising-modding-notes/blob/main/Dumps/VRisingDedicatedServer/Systems/Server/UpdateTree.txt).
  - shortcut `.ds ut`
- `.DumpSystems Code`
  - Generates code snippets for each system, per world. One file per system. [Sample](https://github.com/cheesasaurus/v-rising-modding-notes/blob/main/Dumps/VRisingDedicatedServer/Systems/Server/Code/ProjectM.CastleBuilding.CastleBuildingWorkstationsSystem.cs).
  - shortcut `.ds c`


## Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/ProfuselyViolentProgression). 