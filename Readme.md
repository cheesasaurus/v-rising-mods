monorepo for various V Rising mods

## Repo Structure
- Core: essentially a shared library for mods to use. **The existence of things in here must not cause side effects.**\
No chat commands, no patches altering behavior, etc.
- Mods: contains projects for each mod.
- docs: web things for github pages
- templates: templates for `dotnet new`


## Main setup

1. Setup links to the bepinex libraries and game interops. See the [vendor readme](vendor/README.md) for more information.
2. Restore with `dotnet restore`

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
2. use the template. e.g. `dotnet new vrisingmod3 -n NameOfYourMod --description="Description of your mod" --use-vcf`
3. add the mod's project file to the solution. e.g. `dotnet sln add "./Mods/NameOfYourMod/NameOfYourMod.csproj"`


## Building

`dotnet publish`

### Deploying built mods to local game server

Running `dotnet publish` can automatically copy built plugins to your local server.

Set the environment variable `VRisingServerPath`.\
example value: `E:\Games\SteamLibrary\steamapps\common\VRisingDedicatedServer`

### Bundling additional DLLs
When a mod has some extra dependencies to be distributed alongside the usual plugin dll, they can be specified in the .csproj file for that mod.

e.g.
```
<Target Name="PrepareLibsForDist" AfterTargets="Publish">
  <PropertyGroup>
    <PublishPath>$(OutputPath)/publish</PublishPath>
    <DistAdditionalLibs>$(PublishPath)/LiteDB.dll;</DistAdditionalLibs>
  </PropertyGroup>
</Target>
```

If you want to deploy them to your local game server, you can use `dotnet publish "/p:DeployLibsToo=TRUE"`.\
This only really needs to be done after you've added a new dependency, and requires a restart of the server.


## Distribution
Building creates a thunderstore package for each mod at `Mods/$(ModName)/dist/thunderstore.$(ModName).$(Version).zip`

Additionally, the assemblies from all mods are copied to `dist/modpack/` for convenience.