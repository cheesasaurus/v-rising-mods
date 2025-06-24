using ProjectM;
using Unity.Entities;
using UnityEngine;

namespace VRisingMods.Core.Utilities;


/// <summary>
/// Various utilities for interacting with the Unity ECS world.
/// </summary>
public static class WorldUtil
{
    private static World _clientWorld;
    private static World _serverWorld;
    private static ServerBootstrapSystem _serverBootstrapSystem;

    /// <summary>
    /// Return the Unity ECS World instance used on the server build of VRising.
    /// </summary>
    public static World Server
    {
        get
        {
            if (_serverWorld != null && _serverWorld.IsCreated)
                return _serverWorld;

            _serverWorld = GetWorld("Server")
                ?? throw new System.Exception("There is no Server world (yet). Did you install a server mod on the client?");
            return _serverWorld;
        }
    }

    /// <summary>
    /// Return the Unity ECS World instance used on the client build of VRising.
    /// </summary>
    public static World Client
    {
        get
        {
            if (_clientWorld != null && _clientWorld.IsCreated)
                return _clientWorld;

            _clientWorld = GetWorld("Client_0")
                ?? throw new System.Exception("There is no Client world (yet). Did you install a client mod on the server?");
            return _clientWorld;
        }
    }

    /// <summary>
    /// Return the default Unity ECS World instance. Both client and server use this
    /// to store some "global" systems, like the InputSystem.
    /// </summary>
    public static World Default => World.DefaultGameObjectInjectionWorld;

    /// <summary>
    /// Returns the "game" ECS world for the current instance. This will return either
    /// WorldUtil.Client or WorldUtil.Server, depending on what instance of VRising is running.
    /// </summary>
    public static World Game => IsClient ? Client : Server;

    /// <summary>
    /// Return whether we're currently running on the server build of VRising.
    /// </summary>
    public static bool IsServer => Application.productName == "VRisingServer";

    /// <summary>
    /// Return whether we're currently running on the client build of VRising.
    /// </summary>
    public static bool IsClient => Application.productName == "VRising";

    public static bool IsGameWorldCreated()
    {
        if (_clientWorld != null)
        {
            return _clientWorld.IsCreated;
        }
        if (_serverWorld != null)
        {
            return _serverWorld.IsCreated;
        }

        if (IsClient)
        {
            return GetWorld("Client_0") is not null;
        }
        if (IsServer)
        {
            return GetWorld("Server") is not null;
        }

        return false;
    }

    private static World GetWorld(string name)
    {
        foreach (var world in World.s_AllWorlds)
        {
            if (world.Name == name)
            {
                _serverWorld = world;
                return world;
            }
        }

        return null;
    }

    public static bool IsServerInitialized
    {
        // GameBootstrap.Start
        // LoadPersistenceSystemV2.SetLoadState
        //   ServerStartupState.State loadState
        // ServerRuntimeSettings.StartupState
        get
        {
            if (_serverBootstrapSystem is null)
            {
                _serverBootstrapSystem = Server?.GetExistingSystemManaged<ServerBootstrapSystem>();
            }
            return _serverBootstrapSystem?.ServerIsInitialized ?? false;
        }
    }

}
