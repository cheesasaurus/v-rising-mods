# Vendor folder
Any needed game files should be copied/linked here.
- `vendor/VRisingGameFiles/Bepinex/core`
- `vendor/VRisingGameFiles/Bepinex/interop`

### Linking instead of copying
A powershell script is included (in the project root) to set up symbolic links.

example usage:\
`./Link-GameFiles -Path "F:\Games\SteamLibrary\steamapps\common\VRisingDedicatedServer"`

Using Windows 11's File Explorer, you can open an admin terminal like so:
1) Right click inside the folder to open the context menu.
2) Hold `ctrl` + `shift` and left click on `Open in Terminal`

