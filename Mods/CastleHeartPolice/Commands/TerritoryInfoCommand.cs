using System;
using System.Text;
using Bloodstone.API;
using CastleHeartPolice.Services;
using CastleHeartPolice.Utils;
using ProjectM.Terrain;
using Unity.Mathematics;
using Unity.Transforms;
using VampireCommandFramework;

namespace CastleHeartPolice.Commands;


public class TerritoryInfoCommand {

    [Command("territory-info", shortHand: "ti", description: "get information about the territory your character is in", adminOnly: false)]
    public void Execute(ChatCommandContext ctx) {
        var rulesService = RulesService.Instance;
        var worldPos = WorldPositionOfPlayerCharacter(ctx);
        var blockCoords = CastleTerritoryUtil.BlockCoordinatesFromWorldPosition(worldPos);

        var message = new StringBuilder("Territory Information\n");

        if (CastleTerritoryUtil.TryFindTerritoryContaining(worldPos, out var territoryInfo)) {
            message.AppendLine($"Castle Territory Id: {territoryInfo.TerritoryId}");
            message.AppendLine($"Territory value: {rulesService.LabeledScore(rulesService.TerritoryScore(territoryInfo))}");
            message.AppendLine($"Territory Size (blocks): {territoryInfo.BlockCount}");
        }
        else {
           message.AppendLine("You don't seem to be standing in any castle territory.");
        }

        message.AppendLine($"Character's WorldPosition: {FormatFloat(worldPos.x)} {FormatFloat(worldPos.y)} {FormatFloat(worldPos.z)}");
        message.AppendLine($"Character's BlockCoordinates: {blockCoords.x} {blockCoords.y}");

        ctx.Reply(message.ToString());
    }

    private static float3 WorldPositionOfPlayerCharacter(ChatCommandContext ctx) {
        var translationData = VWorld.Server.EntityManager.GetComponentData<Translation>(ctx.User.LocalCharacter._Entity);
        return translationData.Value;
    }

    private static string FormatFloat(float number) {
        return String.Format("{0:0.00}", number);
    }

}
