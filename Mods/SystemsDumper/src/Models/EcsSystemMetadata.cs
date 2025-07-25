using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Models;


public class EcsSystemMetadata(Il2CppSystem.Type type, SystemHandle systemHandle, SystemTypeIndex systemTypeIndex, EcsSystemCategory category)
{
    public Il2CppSystem.Type Type { get; set; } = type;
    public System.Type TypeIL { get; set; } = null;
    public SystemHandle SystemHandle { get; set; } = systemHandle;
    public SystemTypeIndex SystemTypeIndex { get; set; } = systemTypeIndex;
    public EcsSystemCategory Category { get; set; } = category;
    public SystemAttributes Attributes { get; set; } = new();
    public List<NamedEntityQuery> NamedEntityQueries { get; set; } = [];
    public Exception ExceptionFromQueryFinding { get; set; } = null;
}

public class NamedEntityQuery(string name, EntityQuery query)
{
    public string Name { get; set; }  = name;
    public EntityQuery Query { get; set; }  = query;
}

public class SystemAttributes
{
    public List<UpdateInGroupAttribute> UpdateInGroup { get; set; } = [];
    public List<UpdateBeforeAttribute> UpdateBefore { get; set; } = [];
    public List<UpdateAfterAttribute> UpdateAfter { get; set; } = [];
    public List<CreateBeforeAttribute> CreateBefore { get; set; } = [];
    public List<CreateAfterAttribute> CreateAfter { get; set; } = [];
    public List<DisableAutoCreationAttribute> DisableAutoCreation { get; set; } = [];
    public List<RequireMatchingQueriesForUpdateAttribute> RequireMatchingQueriesForUpdateAttribute { get; set; } = [];

    public bool Any()
    {
        return UpdateInGroup.Any()
            || UpdateBefore.Any()
            || UpdateAfter.Any()
            || CreateBefore.Any()
            || CreateAfter.Any()
            || DisableAutoCreation.Any()
            || RequireMatchingQueriesForUpdateAttribute.Any();
    }
}
