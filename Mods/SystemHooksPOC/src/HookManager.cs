using System;
using Il2CppInterop.Runtime;
using ProjectM.Gameplay.Systems;
using SystemHooksPOC.Hooks;
using Unity.Entities;

namespace SystemHooksPOC;

public static class HookManager
{
    private static bool _initialized = false;

    private static HookRegistry _hookRegistry;

    #region Setup / Teardown

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        _hookRegistry = new HookRegistry();
        _initialized = true;
    }

    public static void UnInitialize()
    {
        if (!_initialized)
        {
            return;
        }
        _hookRegistry = null;
        _initialized = false;
    }

    #endregion


    #region Handlers

    unsafe public static void HandleSystemUpdatePrefix(SystemState* systemState)
    {
        // todo: cabability to skip the update

        var hooks = _hookRegistry.GetHooksInReverseOrderFor_System_OnUpdate_Prefix(systemState->m_SystemTypeIndex);
        foreach (var hook in hooks)
        {
            bool shouldSkip = hook();
        }
    }

    unsafe public static void HandleSystemUpdatePostfix(SystemState* systemState)
    {
        // LogUtil.LogInfo($"Updating! (postfix)");
    }

    #endregion


    #region Hook Registration: System_OnUpdate_Prefix

    public static void RegisterHook_System_OnUpdate_Prefix<T>(Hook_System_OnUpdate_Prefix hook)
    {
        RegisterHook_System_OnUpdate_Prefix(hook, Il2CppType.Of<T>());
    }

    public static void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Type systemType)
    {
        RegisterHook_System_OnUpdate_Prefix(hook, Il2CppType.From(systemType));
    }

    public static void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Il2CppSystem.Type systemType)
    {
        _hookRegistry.RegisterHook_System_OnUpdate_Prefix(hook, systemType);
    }

    #endregion

}

