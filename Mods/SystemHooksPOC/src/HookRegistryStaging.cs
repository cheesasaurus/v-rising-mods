using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using SystemHooksPOC.Hooks;

namespace SystemHooksPOC;

public class HookRegistryStaging
{
    private HookRegistry _hookRegistry;
    private IList<HookRegistry.HookHandle> _registeredHookHandles = new List<HookRegistry.HookHandle>();

    public HookRegistryStaging(HookRegistry hookRegistry)
    {
        _hookRegistry = hookRegistry;
    }

    public void UnregisterRegisteredHooks()
    {
        foreach (var hookHandle in _registeredHookHandles)
        {
            _hookRegistry.UnregisterHook(hookHandle);
        }
        _registeredHookHandles.Clear();
    }

    public void CancelPendingRegistrations()
    {
        // todo: implement
    }

    public void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Il2CppSystem.Type systemType, HookOptions_System_OnUpdate_Prefix options)
    {
        var handle = _hookRegistry.RegisterHook_System_OnUpdate_Prefix(hook, systemType, options);
        _registeredHookHandles.Add(handle);
    }

}
