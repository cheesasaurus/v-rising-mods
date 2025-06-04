using System;
using System.Reflection;
using HarmonyLib;
using SystemHooksPOC.Attributes;
using SystemHooksPOC.ExamplePatches;
using SystemHooksPOC.Hooks;
using VRisingMods.Core.Utilities;

namespace SystemHooksPOC;

public class MainEntryPoint
{
    public string Id { get; }
    public HookRegistryContext HookRegistryContext { get; }

    public MainEntryPoint(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            throw new ArgumentException("id cannot be null or empty");
        }
        Id = id;
        HookRegistryContext = HookManager.NewRegistryContext(id);
    }

    public void RegisterHooks()
    {
        RegisterHooks(Assembly.GetCallingAssembly());
    }

    public void UnregisterHooks()
    {
        HookRegistryContext.UnregisterHooks();
    }

    public void RegisterHooks(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            RegisterHooks(type);
        }
    }

    internal void RegisterHooks(Type type)
    {
        foreach (var methodInfo in type.GetMethods())
        {
            RegisterHooks(methodInfo);
        }
    }

    internal void RegisterHooks(MethodInfo methodInfo)
    {
        RegisterEcsSystemUpdatePrefix(methodInfo);
        // todo: register postfix
    }

    internal bool RegisterEcsSystemUpdatePrefix(MethodInfo methodInfo)
    {
        if (!methodInfo.IsStatic) {
            return false;
        }
        var attribute = methodInfo.GetCustomAttribute<EcsSystemUpdatePrefixAttribute>();
        if (attribute is null)
        {
            return false;
        }
        var hook = methodInfo.CreateDelegate<Hook_System_OnUpdate_Prefix>();
        var options = new HookOptions_System_OnUpdate_Prefix(onlyWhenSystemRuns: attribute.OnlyWhenSystemRuns);
        HookRegistryContext.RegisterHook_System_OnUpdate_Prefix(hook, attribute.SystemType, options);
        var declaringType = methodInfo.DeclaringType;
        // LogUtil.LogDebug($"registered EcsSystemUpdatePrefix hook: {declaringType.FullName}.{methodInfo.Name}");
        return true;
    }

}