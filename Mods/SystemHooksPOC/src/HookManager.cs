using System;
using Il2CppInterop.Runtime;
using ProjectM.Gameplay.Systems;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace SystemHooksPOC;

public static class HookManager
{
    static Type HookedSystemType = typeof(DealDamageSystem);
    private static SystemTypeIndex _hookedSystemTypeIndex = SystemTypeIndex.Null;

    private static int _autoIncrement = 0;
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized)
        {
            return;
        }
        _hookedSystemTypeIndex = TypeManager.GetSystemTypeIndex(Il2CppType.From(HookedSystemType));
        if (_hookedSystemTypeIndex.Equals(SystemTypeIndex.Null))
        {
            LogUtil.LogError($"null sytem type index for {HookedSystemType}");
        }
        else
        {
            LogUtil.LogInfo($"hooked system: {TypeManager.GetSystemType(_hookedSystemTypeIndex).FullName}");
        }
        _initialized = true;
    }

    public static void UnInitialize()
    {
        if (!_initialized)
        {
            return;
        }
        _hookedSystemTypeIndex = SystemTypeIndex.Null;
        _initialized = false;
    }


    public delegate bool HookOnUpdatePrefix();

    unsafe public static void HandleSystemUpdatePrefix(SystemState* systemState)
    {
        if (systemState->m_SystemTypeIndex == _hookedSystemTypeIndex)
        {
            // LogUtil.LogInfo($"Updating {HookedSystemType}! (prefix)");
        }
    }

    unsafe public static void HandleSystemUpdatePostfix(SystemState* systemState)
    {
        if (systemState->m_SystemTypeIndex == _hookedSystemTypeIndex)
        {
            // LogUtil.LogInfo($"Updating {HookedSystemType}! (postfix)");
        }
    }








    public struct HookIndex
    {
        public int Value;
    }
}