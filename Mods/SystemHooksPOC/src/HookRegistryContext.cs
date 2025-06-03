using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using SystemHooksPOC.Hooks;
using static SystemHooksPOC.HookRegistry;


namespace SystemHooksPOC;

public class HookRegistryContext
{
    private HookRegistry _hookRegistry;
    private IList<HookHandle> _registeredHookHandles = new List<HookHandle>();

    public HookRegistryContext(HookRegistry hookRegistry)
    {
        _hookRegistry = hookRegistry;
    }

    public void UnregisterHooks()
    {
        foreach (var hookHandle in _registeredHookHandles)
        {
            _hookRegistry.UnregisterHook(hookHandle);
        }
        _registeredHookHandles.Clear();
    }

    ////////////////////////////////////////////////////////////////////

    #region Hook Registration: System_OnUpdate_Prefix

    public void RegisterHook_System_OnUpdate_Prefix<T>(Hook_System_OnUpdate_Prefix hook)
    {
        RegisterHook_System_OnUpdate_Prefix(hook, Il2CppType.Of<T>());
    }

    public void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Type systemType)
    {
        RegisterHook_System_OnUpdate_Prefix(hook, Il2CppType.From(systemType));
    }

    public void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Il2CppSystem.Type systemType)
    {
        var handle = _hookRegistry.RegisterHook_System_OnUpdate_Prefix(hook, systemType);
        _registeredHookHandles.Add(handle);
    }

    #endregion

    ////////////////////////////////////////////////////////////////////
    

}