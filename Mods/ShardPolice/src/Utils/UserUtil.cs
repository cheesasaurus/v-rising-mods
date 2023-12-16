using System;
using Bloodstone.API;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;

namespace ShardPolice.Utils;


public static class UserUtil {
    public static NativeArray<User> FindAllUsers() {
        var entityManager = VWorld.Server.EntityManager;
        var userType = ComponentType.ReadOnly<User>();
        var query = entityManager.CreateEntityQuery(new ComponentType[]{userType});
        return query.ToComponentDataArray<User>(Allocator.Temp);
    }

    public static bool TryFindUserByName(string characterName, out User? userData) {
        userData = null;
        foreach (var user in FindAllUsers()) {
            if (String.Equals(characterName, user.CharacterName.ToString(), StringComparison.OrdinalIgnoreCase)) {
                userData = user;
                return true;
            }
        }
        return false;
    }

}
