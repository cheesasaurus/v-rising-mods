# SystemsDumper

SystemsDumper is a tool for mod developers.

It dumps information about ECS Systems. For use with both client and server applications.


## Installation

- Install [BepInEx](https://v-rising.thunderstore.io/package/BepInEx/BepInExPack_V_Rising/).
- Install [VampireCommandFramework](https://thunderstore.io/c/v-rising/p/deca/VampireCommandFramework/).
- Extract `SystemsDumper.dll` into `(VRising folder)/BepInEx/plugins`.


## Chat Commands

- `.DumpSystems UpdateTree`
  - Dumps ECS system update hierarchies to files. One file per World. [Sample](https://github.com/cheesasaurus/v-rising-modding-notes/blob/main/Dumps/VRisingDedicatedServer/Systems/Server/UpdateTree.txt)
  - Works on both client and server applications. (client dumps are dumped locally, server dumps are dumped to the server)
  - shortcut `.ds ut`


## Support

Join the [modding community](https://vrisingmods.com/discord).

Post an issue on the [GitHub repository](https://github.com/cheesasaurus/ProfuselyViolentProgression). 