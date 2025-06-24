Param(
   [Parameter(Mandatory)]
   $modname
)

Write-Output "Preparing to create new mod '$modname'"

# install latest version of mod template
Set-Location "./templates"
dotnet new install VRisingMods.ModTemplate3 --force
Set-Location "../"

# create mod
Set-Location "./Mods"
dotnet new vrisingmod3 -n "$modname" --description="Description of your mod" --use-vcf
Write-Output "Created new mod at ./Mods/$modname/"
Set-Location "../"

dotnet sln add "./Mods/$modname/$modname.csproj"