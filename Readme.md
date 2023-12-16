monorepo for various V Rising mods


## Deploying built mods to local game server
You can automatically copy built plugins to your local server, by setting the environment variable `VRisingServerPath`.\
example value: `E:\Games\SteamLibrary\steamapps\common\VRisingDedicatedServer`


## Setting up a new mod

There's a powershell script to easily do this.\
e.g. `./newmod NameOfMyMod`

### If you don't have powershell
Preparation (only required once):
1. cd to `templates`
2. install the mod template. `dotnet new install VRisingMods.ModTemplate`
3. cd back to the project root when done.

After preparation
1. cd to `Mods`
2. use the template. e.g. `dotnet new vrisingmod2 -n NameOfYourMod --description="Description of your mod" --use-bloodstone --use-vcf`
3. add the mod's project file to the solution. e.g. `dotnet sln add "./Mods/NameOfYourMod/NameOfYourMod.csproj"`


## Notes
- a `<ProjectReference>` to a "class library" project requires the library dll to be shipped alongside the main dll. not ideal
- but an `<Import>` of a "shared" project does what I want

### resources
- https://learn.microsoft.com/en-us/dotnet/core/tutorials/library-with-visual-studio?pivots=dotnet-6-0