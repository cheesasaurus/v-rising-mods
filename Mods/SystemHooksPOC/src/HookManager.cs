using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using SystemHooksPOC.Hooks;
using Unity.Entities;

namespace SystemHooksPOC;

public static class HookManager
{
    public static Bus Bus = new();
    private static bool _initialized = false;
    private static bool _isGameReadyForRegistration = false;

    private static HookRegistry _hookRegistry;

    ////////////////////////////////////////////////////////////////////

    #region Setup / Teardown

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        _hookRegistry = new HookRegistry();
        Bus.GameReadyForRegistration += HandleGameReadyForRegistration;
        _initialized = true;
    }

    public static void UnInitialize()
    {
        if (!_initialized)
        {
            return;
        }
        Bus.GameReadyForRegistration -= HandleGameReadyForRegistration;
        _hookRegistry = null;
        _isGameReadyForRegistration = false;
        _initialized = false;
    }

    #endregion

    ////////////////////////////////////////////////////////////////////

    #region Handlers

    private static void HandleGameReadyForRegistration()
    {
        _isGameReadyForRegistration = true;
    }

    private static Dictionary<SystemTypeIndex, bool> _restoreEnabledAfterPrefixSkip_System_OnUpdate = new();
    private static Dictionary<SystemTypeIndex, bool> _didPrefixExpectSystemToRun = new();

    unsafe public static void HandleSystemUpdatePrefix(SystemState* systemState)
    {
        var systemTypeIndex = systemState->m_SystemTypeIndex;
        var hookWrappers = _hookRegistry.GetHooksInReverseOrderFor_System_OnUpdate_Prefix(systemTypeIndex);
        bool wouldRunSystem = systemState->Enabled && systemState->ShouldRunSystem();

        bool shouldStopExecutingPrefixesAndSkipTheOriginal = false;
        foreach (var hookWrapper in hookWrappers)
        {
            if (!wouldRunSystem && hookWrapper.Options.OnlyWhenSystemRuns)
            {
                continue;
            }

            if (false == hookWrapper.Hook())
            {
                shouldStopExecutingPrefixesAndSkipTheOriginal = true;
                break;
            }
        }

        if (shouldStopExecutingPrefixesAndSkipTheOriginal)
        {
            _restoreEnabledAfterPrefixSkip_System_OnUpdate[systemTypeIndex] = systemState->Enabled;
            systemState->Enabled = false;
        }
        _didPrefixExpectSystemToRun[systemTypeIndex] = wouldRunSystem && !shouldStopExecutingPrefixesAndSkipTheOriginal;
    }

    unsafe public static void HandleSystemUpdatePostfix(SystemState* systemState)
    {
        // todo: onlyWhenSystemRuns option

        var systemTypeIndex = systemState->m_SystemTypeIndex;
        if (_restoreEnabledAfterPrefixSkip_System_OnUpdate.ContainsKey(systemTypeIndex))
        {
            systemState->Enabled = _restoreEnabledAfterPrefixSkip_System_OnUpdate[systemTypeIndex];
            _restoreEnabledAfterPrefixSkip_System_OnUpdate.Remove(systemTypeIndex);
        }

        // todo: run postfix hooks
    }

    #endregion

    ////////////////////////////////////////////////////////////////////

    public static HookRegistryContext NewRegistryContext(string id)
    {
        var staging = new HookRegistryStaging(id, _hookRegistry, Bus, _isGameReadyForRegistration);
        return new HookRegistryContext(id, staging);
    }
    
}

