using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using SystemHooksPOC.Hooks;
using VRisingMods.Core.Utilities;

namespace SystemHooksPOC;

public class HookRegistryStaging
{
    private HookRegistry _hookRegistry;
    private Bus _bus;
    private bool _canRegister = false;

    private List<HookRegistry.HookHandle> _registeredHookHandles = new();
    private Queue<RegistryEntry_System_OnUpdate_Prefix> _pendingRegistrations_System_OnUpdate_Prefix = new();

    public HookRegistryStaging(HookRegistry hookRegistry, Bus bus, bool isGameReadyForRegistration)
    {
        _hookRegistry = hookRegistry;
        _canRegister = isGameReadyForRegistration;
        _bus = bus;
        _bus.GameReadyForRegistration += HandleGameReadyForRegistration;
        // todo: way to cleanup event handler
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
        _pendingRegistrations_System_OnUpdate_Prefix.Clear();
    }

    private void HandleGameReadyForRegistration()
    {
        _canRegister = true;
        LogUtil.LogDebug("processing pending registrations");
        ProcessPendingRegistrations_System_OnUpdate_Prefix();
    }

    private void ProcessPendingRegistrations_System_OnUpdate_Prefix()
    {
        while (_pendingRegistrations_System_OnUpdate_Prefix.Count != 0)
        {
            var registryEntry = _pendingRegistrations_System_OnUpdate_Prefix.Dequeue();
            try
            {
                RegisterHook_System_OnUpdate_Prefix(registryEntry);
            }
            catch (Exception ex)
            {
                LogUtil.LogError(ex);
            }
        }
    }

    public void RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Il2CppSystem.Type systemType, HookOptions_System_OnUpdate_Prefix options)
    {
        var registryEntry = new RegistryEntry_System_OnUpdate_Prefix(hook, systemType, options);
        if (_canRegister)
        {
            RegisterHook_System_OnUpdate_Prefix(registryEntry);
        }
        else
        {
            LogUtil.LogDebug("added pending registration");
            _pendingRegistrations_System_OnUpdate_Prefix.Enqueue(registryEntry);
        }
    }

    private void RegisterHook_System_OnUpdate_Prefix(RegistryEntry_System_OnUpdate_Prefix entry)
    {
        var handle = _hookRegistry.RegisterHook_System_OnUpdate_Prefix(entry.Hook, entry.SystemType, entry.Options);
        _registeredHookHandles.Add(handle);
    }

    // todo: maybe better for HookRegistry to use these RegistryEntry records too
    private record RegistryEntry_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix Hook, Il2CppSystem.Type SystemType, HookOptions_System_OnUpdate_Prefix Options);

}
