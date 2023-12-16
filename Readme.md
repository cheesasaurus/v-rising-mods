monorepo for various V Rising mods

can automatically copy built plugins to your local server, by setting the environment variable `VRisingServerPath`.
example value: `E:\Games\SteamLibrary\steamapps\common\VRisingDedicatedServer`


### notes
- a `<ProjectReference>` to a "class library" project requires the library dll to be shipped alongside the main dll. not ideal
- but an `<Import>` of a "shared" project does what I want

### resources
- https://learn.microsoft.com/en-us/dotnet/core/tutorials/library-with-visual-studio?pivots=dotnet-6-0