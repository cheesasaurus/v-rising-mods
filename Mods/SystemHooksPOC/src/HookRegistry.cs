using System;
using System.Collections.Generic;
using SystemHooksPOC.Hooks;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace SystemHooksPOC;

using HooksFor_System_OnUpdate_Prefix = Dictionary<HookRegistry.HookHandle, HookRegistry.HookWrapper_System_OnUpdate_Prefix>;

public class HookRegistry
{
    private int _autoIncrement = 0;

    private Dictionary<SystemTypeIndex, HooksFor_System_OnUpdate_Prefix> _hooksBySystemFor_System_OnUpdate_Prefix = new();
    private ICollection<HookWrapper_System_OnUpdate_Prefix> _emptyCollectionFor_System_OnUpdate_Prefix = Array.Empty<HookWrapper_System_OnUpdate_Prefix>();

    public void UnregisterHook(HookHandle hookHandle)
    {
        switch (hookHandle.HookType)
        {
            case HookType.System_OnUpdate_Prefix:
                _hooksBySystemFor_System_OnUpdate_Prefix[hookHandle.SystemTypeIndex].Remove(hookHandle);
                break;
            case HookType.System_OnUpdate_Postfix:
                // todo. would need to add a way to register first
                break;
        }
    }

    public HookHandle RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Il2CppSystem.Type systemType, HookOptions_System_OnUpdate_Prefix options)
    {
        var systemTypeIndex = TypeManager.GetSystemTypeIndex(systemType);
        if (systemTypeIndex.Equals(SystemTypeIndex.Null))
        {
            throw new Exception($"null sytem type index for {systemType.FullName}");
        }
        else
        {
            LogUtil.LogDebug($"registered OnUpdate prefix hook for: {TypeManager.GetSystemType(systemTypeIndex).FullName}");
        }

        var handle = new HookHandle()
        {
            Value = ++_autoIncrement,
            HookType = HookType.System_OnUpdate_Prefix,
            SystemTypeIndex = systemTypeIndex,
        };

        // ensure we have a registry for that system
        // todo: we have a complete mess here. in the actual implementation (not the POC) this should be cleaned up
        HooksFor_System_OnUpdate_Prefix hooksForSystem;
        if (_hooksBySystemFor_System_OnUpdate_Prefix.ContainsKey(systemTypeIndex))
        {
            hooksForSystem = _hooksBySystemFor_System_OnUpdate_Prefix[systemTypeIndex];
        }
        else
        {
            hooksForSystem = new HooksFor_System_OnUpdate_Prefix();
            _hooksBySystemFor_System_OnUpdate_Prefix.Add(systemTypeIndex, hooksForSystem);
        }

        // register the hook
        hooksForSystem.Add(handle, new HookWrapper_System_OnUpdate_Prefix()
        {
            Hook = hook,
            Options = options
        });

        return handle;
    }

    // todo: more performant way of dealing with this in the actual implementation (not the POC)
    public ICollection<HookWrapper_System_OnUpdate_Prefix> GetHooksInReverseOrderFor_System_OnUpdate_Prefix(SystemTypeIndex systemTypeIndex)
    {
        var lookup = _hooksBySystemFor_System_OnUpdate_Prefix;
        if (!lookup.ContainsKey(systemTypeIndex))
        {
            return _emptyCollectionFor_System_OnUpdate_Prefix;
        }
        // todo: OrderedDictionary kind of thing. Problem is that it's not generic, so just using a regular Dictionary for now
        //return lookup[systemTypeIndex].Values.Reverse();
        return lookup[systemTypeIndex].Values; // todo: actually reverse it, and also optimize things
    }

    public struct HookHandle
    {
        public int Value;
        public HookType HookType;
        public SystemTypeIndex SystemTypeIndex;
    }

    public enum HookType
    {
        System_OnUpdate_Prefix,
        System_OnUpdate_Postfix
    }

    public class HookWrapper_System_OnUpdate_Prefix
    {
        public Hook_System_OnUpdate_Prefix Hook;
        public HookOptions_System_OnUpdate_Prefix Options;
    }

    public class HookWrapper_System_OnUpdate_Postfix
    {
        public Hook_System_OnUpdate_Postfix Hook;
        public HookOptions_System_OnUpdate_Postfix Options;
    }

}