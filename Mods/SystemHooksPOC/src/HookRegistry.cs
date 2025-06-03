using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using SystemHooksPOC.Hooks;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace SystemHooksPOC;

using HooksFor_System_OnUpdate_Prefix = Dictionary<HookRegistry.HookHandle, Hook_System_OnUpdate_Prefix>;

public class HookRegistry
{
    private int _autoIncrement = 0;

    private Dictionary<SystemTypeIndex, HooksFor_System_OnUpdate_Prefix> _hooksBySystemFor_System_OnUpdate_Prefix = new();
    private IEnumerable<Hook_System_OnUpdate_Prefix> _emptyEnumerableFor_System_OnUpdate_Prefix = Enumerable.Empty<Hook_System_OnUpdate_Prefix>();

    public HookHandle RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Type systemType)
    {
        return RegisterHook_System_OnUpdate_Prefix(hook, Il2CppType.From(systemType));
    }

    public HookHandle RegisterHook_System_OnUpdate_Prefix(Hook_System_OnUpdate_Prefix hook, Il2CppSystem.Type systemType)
    {
        var systemTypeIndex = TypeManager.GetSystemTypeIndex(systemType);
        if (systemTypeIndex.Equals(SystemTypeIndex.Null))
        {
            LogUtil.LogError($"null sytem type index for {systemType.FullName}");
        }
        else
        {
            LogUtil.LogInfo($"hooked system: {TypeManager.GetSystemType(systemTypeIndex).FullName}");
        }

        var handle = new HookHandle()
        {
            Value = ++_autoIncrement,
            HookType = HookType.System_OnUpdate_Prefix
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

        // actually register the hook
        hooksForSystem.Add(handle, hook);

        return handle;
    }

    // todo: more performant way of dealing with this in the actual implementation (not the POC)
    public IEnumerable<Hook_System_OnUpdate_Prefix> GetHooksInReverseOrderFor_System_OnUpdate_Prefix(SystemTypeIndex systemTypeIndex)
    {
        var lookup = _hooksBySystemFor_System_OnUpdate_Prefix;
        if (!lookup.ContainsKey(systemTypeIndex))
        {
            return _emptyEnumerableFor_System_OnUpdate_Prefix;
        }
        // todo: OrderedDictionary kind of thing. Problem is that it's not generic, so just using a regular Dictionary for now
        return lookup[systemTypeIndex].Values.Reverse();
    }

    public struct HookHandle
    {
        public int Value;
        public HookType HookType;
    }

    public enum HookType
    {
        System_OnUpdate_Prefix,
        System_OnUpdate_Postfix
    }
}