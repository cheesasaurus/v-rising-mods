using System;

namespace SystemHooksPOC.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class EcsSystemUpdatePrefixAttribute : Attribute
{
    public Type SystemType { get; }
    public bool OnlyWhenSystemRuns { get; }

    public EcsSystemUpdatePrefixAttribute(Type systemType, bool onlyWhenSystemRuns = true)
    {
        SystemType = systemType;
        OnlyWhenSystemRuns = onlyWhenSystemRuns;
    }

}
