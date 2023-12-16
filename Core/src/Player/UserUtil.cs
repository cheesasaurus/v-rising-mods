using System;
using System.Collections.Generic;
using Bloodstone.API;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace VRisingMods.Core.Player;


public static class UserUtil {
    public static List<UserModel> FindAllUsers() {
        var entityManager = VWorld.Server.EntityManager;
        var userType = ComponentType.ReadOnly<User>();
        var query = entityManager.CreateEntityQuery(new ComponentType[]{userType});

        var entities = query.ToEntityArray(Allocator.Temp);
        var users = query.ToComponentDataArray<User>(Allocator.Temp);

        var userModels = new List<UserModel>();
        for (var i = 0; i < entities.Length; i++) {
            userModels.Add(new UserModel {
                Entity = entities[i],
                User = users[i],
            });
        }
        return userModels;
    }

    public static bool TryFindUserByName(string characterName, out UserModel userModel) {
        foreach (var user in FindAllUsers()) {
            if (String.Equals(characterName, user.User.CharacterName.ToString(), StringComparison.OrdinalIgnoreCase)) {
                userModel = user;
                return true;
            }
        }
        userModel = default;
        return false;
    }

    public static bool TryFindUserByPlatformId(ulong platformId, out UserModel userModel) {
        foreach (var user in FindAllUsers()) {
            if (platformId.Equals(user.User.PlatformId)) {
                userModel = user;
                return true;
            }
        }
        userModel = default;
        return false;
    }

}
