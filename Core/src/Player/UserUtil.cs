using System;
using System.Collections.Generic;
using ProjectM.Network;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace VRisingMods.Core.Player;


public static class UserUtil
{
    public static List<UserModel> FindAllUsers()
    {
        var entityManager = WorldUtil.Server.EntityManager;
        var userType = ComponentType.ReadOnly<User>();
        var query = entityManager.CreateEntityQuery(new ComponentType[] { userType });

        var entities = query.ToEntityArray(Allocator.Temp);

        var userModels = new List<UserModel>();
        foreach (var userEntity in entities)
        {
            userModels.Add(new UserModel
            {
                Entity = userEntity,
                User = entityManager.GetComponentData<User>(userEntity),
            });
        }
        return userModels;
    }

    public static bool TryFindUserByName(string characterName, out UserModel userModel)
    {
        foreach (var user in FindAllUsers())
        {
            if (String.Equals(characterName, user.User.CharacterName.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                userModel = user;
                return true;
            }
        }
        userModel = default;
        return false;
    }

    public static bool TryFindUserByPlatformId(ulong platformId, out UserModel userModel)
    {
        foreach (var user in FindAllUsers())
        {
            if (platformId.Equals(user.User.PlatformId))
            {
                userModel = user;
                return true;
            }
        }
        userModel = default;
        return false;
    }

    // This is not the proper way to elevate a user to admin.
    // There are various caches/lookups to update, properties to set, network events to spawn, etc.
    // But I cannot find a utility/helper to handle all of this.
    // So we do something hacky here, knowing that plugins check User.IsAdmin for chat command authorization.
    public static void HaxSetIsAdminForPluginChatCommands(Entity userEntity, bool isAdmin)
    {
        var entityManager = WorldUtil.Server.EntityManager;
        if (entityManager.TryGetComponentData<User>(userEntity, out var user))
        {
            user.IsAdmin = isAdmin;
            entityManager.SetComponentData(userEntity, user);
        }
    }

    public static bool IsAdminForPluginChatCommands(Entity userEntity)
    {
        var entityManager = WorldUtil.Server.EntityManager;
        if (entityManager.TryGetComponentData<User>(userEntity, out var user))
        {
            return user.IsAdmin;
        }
        return false;
    }

}
