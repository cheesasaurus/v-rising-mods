using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using SystemHooksPOC.Hooks;


namespace SystemHooksPOC;

public class HookRegistryContext
{
    private HookRegistryStaging _hookRegistryStaging;
    private string _id;

    public HookRegistryContext(string id, HookRegistryStaging hookRegistryStaging)
    {
        _id = id;
        _hookRegistryStaging = hookRegistryStaging;
    }

    public void UnregisterHooks()
    {
        _hookRegistryStaging.CancelPendingRegistrations();
        _hookRegistryStaging.UnregisterRegisteredHooks();
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
        _hookRegistryStaging.RegisterHook_System_OnUpdate_Prefix(hook, systemType, options);
    }

    #endregion

    ////////////////////////////////////////////////////////////////////
    

}