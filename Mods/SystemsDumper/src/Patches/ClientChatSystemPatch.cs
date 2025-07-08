using cheesasaurus.VRisingMods.SystemsDumper.Commands;
using HarmonyLib;
using ProjectM.UI;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Patches;

[HarmonyPatch]
internal static class ClientChatSystemPatch
{
    [HarmonyPatch(typeof(ClientChatSystem), nameof(ClientChatSystem.ParseCommand))]
    [HarmonyPrefix]
    public static void AbilityRunScriptsSystem_Prefix(ClientChatSystem __instance, string text)
    {
        DumpCommands_Client.HandleChatMessageSubmit(text);
    }
}
