monorepo for various V Rising mods

can automatically copy built plugins to your local server, by setting the environment variable `VRisingServerPath`.
example value: `E:\Games\SteamLibrary\steamapps\common\VRisingDedicatedServer`


## setup a new mod
Preparation (only required once): cd to `templates` and install the mod template. `dotnet new install VRisingMods.ModTemplate`
cd back to the project root when done.

1. cd to `Mods` and then use the template. e.g. `dotnet new vrisingmod2 -n NameOfYourMod --description "Description of your mod" --use-bloodstone --use-vcf`


## notes
- a `<ProjectReference>` to a "class library" project requires the library dll to be shipped alongside the main dll. not ideal
- but an `<Import>` of a "shared" project does what I want

### resources
- https://learn.microsoft.com/en-us/dotnet/core/tutorials/library-with-visual-studio?pivots=dotnet-6-0