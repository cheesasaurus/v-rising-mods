#Requires -RunAsAdministrator

Param(
   [Parameter(Mandatory)]
   $Path
)

$VendorPathBepInEx = "vendor/VRisingGameFiles/BepInEx"

if (!(Test-Path -Path $VendorPathBepInEx)) {
   New-Item -ItemType Directory -Path $VendorPathBepInEx
}

New-Item -ItemType SymbolicLink -Path "vendor/VRisingGameFiles/BepInEx/core" -Value "$Path/BepInEx/core"
New-Item -ItemType SymbolicLink -Path "vendor/VRisingGameFiles/BepInEx/interop" -Value "$Path/BepInEx/interop"
