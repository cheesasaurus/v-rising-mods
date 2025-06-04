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

    public void RegisterHook_System_OnUpdate_Prefix<TSystemType>(Hook_System_OnUpdate_Prefix hook)
    {
        var options = HookOptions_System_OnUpdate_Prefix.Default;
        RegisterHook_System_OnUpdate_Prefix<TSystemType>(hook, options);
    }

    public void RegisterHook_System_OnUpdate_Prefix<TSystemType>(Hook_System_OnUpdate_Prefix hook, HookOptions_System_OnUpdate_Prefix options)
    {
        RegisterHook_System_OnUpdate_Prefix(hook, Il2CppType.Of<TSystemType>(), options);
    }

    public void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Type systemType)
    {
        var options = HookOptions_System_OnUpdate_Prefix.Default;
        RegisterHook_System_OnUpdate_Prefix(hook, systemType, options);
    }

    public void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Type systemType, HookOptions_System_OnUpdate_Prefix options)
    {
        RegisterHook_System_OnUpdate_Prefix(hook, Il2CppType.From(systemType), options);
    }

    public void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Il2CppSystem.Type systemType)
    {
        var options = HookOptions_System_OnUpdate_Prefix.Default;
        RegisterHook_System_OnUpdate_Prefix(hook, systemType, options);
    }

    public void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Il2CppSystem.Type systemType, HookOptions_System_OnUpdate_Prefix options)
    {
        var handle = _hookRegistry.RegisterHook_System_OnUpdate_Prefix(hook, systemType, options);
        _registeredHookHandles.Add(handle);
    }

    #endregion

    ////////////////////////////////////////////////////////////////////
    

}