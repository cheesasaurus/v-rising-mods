using System;
using System.Collections.Generic;
using ProjectM;
using ProjectM.CastleBuilding;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace VRisingMods.Core.CastleTerritory;

public static class CastleHeartUtil {

    // Find castle hearts belonging to a single player
    public static List<Entity> FindCastleHeartsOfPlayer(Entity character) {
        var entityManager = WorldUtil.Server.EntityManager;
        var playerCharacterData = entityManager.GetComponentData<PlayerCharacter>(character); 
        var playerHearts = new List<Entity>();

        var query = entityManager.CreateEntityQuery(new EntityQueryDesc() {
            All = new ComponentType[] {
                ComponentType.ReadOnly<CastleHeart>(),
            },
        });
        var heartEntities = query.ToEntityArray(Allocator.Temp);
        foreach (var heartEntity in heartEntities) {
            var heartOwner = entityManager.GetComponentData<UserOwner>(heartEntity);
            if (playerCharacterData.UserEntity.Equals(heartOwner.Owner._Entity)) {
                playerHearts.Add(heartEntity);
            }
        }
        return playerHearts;
    }

    // Find castle hearts belonging to a player or players in their clan
    // (a clanless player is in their own Team with no other players)
    public static List<Entity> FindCastleHeartsOfPlayerTeam(Entity character) {
        var entityManager = WorldUtil.Server.EntityManager;
        var playerTeam = entityManager.GetComponentData<Team>(character);
        return FindCastleHeartsOfTeam(playerTeam.Value);
    }

    public static List<Entity> FindCastleHeartsOfClan(Entity clanTeam) {
        var entityManager = WorldUtil.Server.EntityManager;
        var clanTeamData = entityManager.GetComponentData<ClanTeam>(clanTeam);
        return FindCastleHeartsOfTeam(clanTeamData.TeamValue);
    }

    public static List<Entity> FindCastleHeartsOfTeam(int teamId) {
        var entityManager = WorldUtil.Server.EntityManager;
        var clanHearts = new List<Entity>();

        var query = entityManager.CreateEntityQuery(new EntityQueryDesc() {
            All = new ComponentType[] {
                ComponentType.ReadOnly<CastleHeart>(),
                ComponentType.ReadOnly<Team>(),
            },
        });
        var heartEntities = query.ToEntityArray(Allocator.Temp);
        foreach (var heartEntity in heartEntities) {
            var heartTeam = entityManager.GetComponentData<Team>(heartEntity);
            if (teamId.Equals(heartTeam.Value)) {
                clanHearts.Add(heartEntity);
            }
        }
        return clanHearts;
    }

    public static Entity FindCastleHeartById(NetworkId heartId) {
        var entityManager = WorldUtil.Server.EntityManager;
        var query = entityManager.CreateEntityQuery(new EntityQueryDesc() {
            All = new ComponentType[] {
                ComponentType.ReadOnly<CastleHeart>(),
                ComponentType.ReadOnly<NetworkId>(),
            },
        });
        var heartEntities = query.ToEntityArray(Allocator.Temp);
        foreach (var heartEntity in heartEntities) {
            var networkId = entityManager.GetComponentData<NetworkId>(heartEntity);
            if (heartId.Equals(networkId)) {
                return heartEntity;
            }
        }
        throw new Exception();
    }

}
