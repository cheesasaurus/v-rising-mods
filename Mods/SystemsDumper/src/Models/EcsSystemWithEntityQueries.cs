using System;
using System.Collections.Generic;
using Unity.Entities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Models;

public class EcsSystemWithEntityQueries(EcsSystemCategory category, SystemHandle systemHandle, Il2CppSystem.Type type)
{
    public EcsSystemCategory Category { get; set; } = category;
    public Il2CppSystem.Type Type { get; set; } = type;
    public SystemHandle SystemHandle { get; set; } = systemHandle;
    public List<NamedEntityQuery> NamedEntityQueries { get; set; } = [];
    public Exception ExceptionFromQueryFinding { get; set; } = null;
}

public class NamedEntityQuery(string name, EntityQuery query)
{
    public string Name { get; set; }  = name;
    public EntityQuery Query { get; set; }  = query;
}
