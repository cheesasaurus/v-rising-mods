using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Cpp2IL.Core.Extensions;
using Unity.Entities;
using static Unity.Entities.ComponentType;

namespace cheesasaurus.VRisingMods.SystemsDumper.Dumpers;

class EntityQueriesDumper
{
    private int _spacesPerIndent;

    public EntityQueriesDumper(int spacesPerIndent = 4)
    {
        _spacesPerIndent = spacesPerIndent;
    }

    public string ListAllQueries(World world, IEnumerable<EcsSystemWithEntityQueries> systems)
    {
        var sb = new StringBuilder();
        var hr = "=".Repeat(72);
        var sectionSeparator = $"\n{hr}\n\n\n";

        sb.AppendLine($"Information about Systems' entity queries in world: {world.Name}");

        foreach (var system in systems)
        {
            sb.Append(sectionSeparator);
            AppendSystemSection(sb, system);
        }

        return sb.ToString();
    }

    private void AppendSystemSection(StringBuilder sb, EcsSystemWithEntityQueries system)
    {
        sb.AppendLine($"{system.Type.FullName} ({DescribeCategory(system)})");
        sb.AppendLine();
        sb.AppendLine($"Entity Queries: {system.NamedEntityQueries.Count}");
        sb.AppendLine();
        sb.AppendLine("-------------------------------------\n\n");

        if (system.ExceptionFromQueryFinding is not null)
        {
            sb.AppendLine($"Was unable to find queries due to an error:");
            sb.AppendLine();
            sb.AppendLine(system.ExceptionFromQueryFinding.ToString());
            sb.AppendLine();
            sb.AppendLine();
        }

        foreach (var namedQuery in system.NamedEntityQueries)
        {
            var isValid = ValidateAndAppendProblems(sb, namedQuery, system);
            if (isValid)
            {
                AppendQueryDescSnippet(sb, namedQuery, system);
            }
            sb.AppendLine();
            sb.AppendLine();
        }
    }

    private string DescribeCategory(EcsSystemWithEntityQueries system)
    {
        switch (system.Category)
        {
            case EcsSystemCategory.Group:
                return "ComponentSystemGroup";
            case EcsSystemCategory.Base:
                return "ComponentSystemBase";
            case EcsSystemCategory.Unmanaged:
                return "ISystem";
            default:
            case EcsSystemCategory.Unknown:
                return "<unknown system type>";
        }
    }

    private bool ValidateAndAppendProblems(StringBuilder sb, NamedEntityQuery namedQuery, EcsSystemWithEntityQueries system)
    {
        EntityQueryDesc queryDesc;
        try
        {
            if (system.Category == EcsSystemCategory.Unmanaged)
            {
                // todo: figure out how to access. it just crashes with no exceptions :(
                sb.AppendLine($"Skipping {namedQuery.Name} (on unmanaged system)");
                return false;
            }

            if (!namedQuery.Query.IsCacheValid)
            {
                // todo: investigate this. is there a way to trigger the cacheing?
                // for example, going into the blackbrew fight and dumping,
                // gives us more usable queries on ProjectM.ProfessorCoilSystem_Server_Spawn.
                sb.AppendLine($"could not access {namedQuery.Name} | IsCacheValid: {namedQuery.Query.IsCacheValid}");
                return false;
            }

            // check that we can access query desc
            queryDesc = namedQuery.Query.GetEntityQueryDesc();
        }
        catch (Exception ex0)
        {
            sb.AppendLine($"could not access {namedQuery.Name}");
            sb.AppendLine(ex0.ToString());
            return false;
        }

        return true;
    }

    // The snippet created here sadly won't work.
    // We have to pass the il2cpp types, which cannot be done with generics.
    //
    // There are methods we could use, such as:
    //     WithPresent(ComponentType* componentTypes, int count)
    //
    // But we cannot reference things created inline - they have to point to a variable. eg:
    //     var component = new ComponentType(Il2CppType.Of<ProjectM.Network.FromCharacter>(), ComponentType.AccessMode.ReadOnly);
    //     var _EventQuery = new EntityQueryBuilder(Allocator.Temp)
    //        .WithPresent(&component, 1)
    //        .Build(entityManager)
    //  
    // which when using lots of components with a variety of filters, makes it rather unreadable.
    // The idea is to tell at a glance how the components are being used, and to also have a usable snippet.
    unsafe private void AppendQueryBuilderSnippet(StringBuilder sb, NamedEntityQuery namedQuery, EcsSystemWithEntityQueries system)
    {
        var indent = new String(' ', _spacesPerIndent);
        var queryDesc = namedQuery.Query.GetEntityQueryDesc();

        sb.AppendLine($"var {namedQuery.Name} = new EntityQueryBuilder(Allocator.Temp)");

        // All
        foreach (var componentType in queryDesc.All)
        {
            if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
            {
                sb.AppendLine($"{indent}.WithAllRW<{componentType.GetManagedType().FullName}>()");
            }
            else
            {
                sb.AppendLine($"{indent}.WithAll<{componentType.GetManagedType().FullName}>()");
            }
        }

        // Any
        foreach (var componentType in queryDesc.Any)
        {
            if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
            {
                sb.AppendLine($"{indent}.WithAnyRW<{componentType.GetManagedType().FullName}>()");
            }
            else
            {
                sb.AppendLine($"{indent}.WithAny<{componentType.GetManagedType().FullName}>()");
            }
        }

        // None
        foreach (var componentType in queryDesc.None)
        {
            sb.AppendLine($"{indent}.WithNone<{componentType.GetManagedType().FullName}>()");
        }

        // Disabled
        foreach (var componentType in queryDesc.Disabled)
        {
            if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
            {
                sb.AppendLine($"{indent}.WithDisabledRW<{componentType.GetManagedType().FullName}>()");
            }
            else
            {
                sb.AppendLine($"{indent}.WithDisabled<{componentType.GetManagedType().FullName}>()");
            }
        }

        // Absent
        foreach (var componentType in queryDesc.Absent)
        {
            sb.AppendLine($"{indent}.WithAbsent<{componentType.GetManagedType().FullName}>()");
        }

        // Present
        foreach (var componentType in queryDesc.Present)
        {
            if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
            {
                sb.AppendLine($"{indent}.WithPresentRW<{componentType.GetManagedType().FullName}>()");
            }
            else
            {
                sb.AppendLine($"{indent}.WithPresent<{componentType.GetManagedType().FullName}>()");
            }
        }

        // todo: options

        sb.AppendLine($"{indent}.Build(entityManager);");
    }
    
    // EntityQueryDesc is not the way unity recommends, but it is a hell of a lot more convenient with our il2cpp restrictions.
    unsafe private void AppendQueryDescSnippet(StringBuilder sb, NamedEntityQuery namedQuery, EcsSystemWithEntityQueries system)
    {
        var indent = new String(' ', _spacesPerIndent);
        var queryDesc = namedQuery.Query.GetEntityQueryDesc();

        sb.AppendLine($"var {namedQuery.Name} = entityManager.CreateEntityQuery(new EntityQueryDesc()");
        sb.AppendLine("{");

        // All
        if (queryDesc.All.Any())
        {
            sb.AppendLine($"{indent}All = new ComponentType[] {{");
            foreach (var componentType in queryDesc.All)
            {
                var typeName = componentType.GetManagedType().FullName;
                if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
                {
                    sb.AppendLine($"{indent}{indent}ComponentType.ReadWrite<{typeName}>(),");
                }
                else
                {
                    sb.AppendLine($"{indent}{indent}ComponentType.ReadOnly<{typeName}>(),");
                }
            }
            sb.AppendLine($"{indent}}},");
        }

        // Any
        if (queryDesc.Any.Any())
        {
            sb.AppendLine($"{indent}Any = new ComponentType[] {{");
            foreach (var componentType in queryDesc.Any)
            {
                var typeName = componentType.GetManagedType().FullName;
                if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
                {
                    sb.AppendLine($"{indent}{indent}ComponentType.ReadWrite<{typeName}>(),");
                }
                else
                {
                    sb.AppendLine($"{indent}{indent}ComponentType.ReadOnly<{typeName}>(),");
                }
            }
            sb.AppendLine($"{indent}}},");
        }

        // None
        if (queryDesc.None.Any())
        {
            sb.AppendLine($"{indent}None = new ComponentType[] {{");
            foreach (var componentType in queryDesc.None)
            {
                var typeName = componentType.GetManagedType().FullName;
                sb.AppendLine($"{indent}{indent}ComponentType.ReadOnly<{typeName}>(),");
            }
            sb.AppendLine($"{indent}}},");
        }

        // Disabled
        if (queryDesc.Disabled.Any())
        {
            sb.AppendLine($"{indent}Disabled = new ComponentType[] {{");
            foreach (var componentType in queryDesc.Disabled)
            {
                var typeName = componentType.GetManagedType().FullName;
                if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
                {
                    sb.AppendLine($"{indent}{indent}ComponentType.ReadWrite<{typeName}>(),");
                }
                else
                {
                    sb.AppendLine($"{indent}{indent}ComponentType.ReadOnly<{typeName}>(),");
                }
            }
            sb.AppendLine($"{indent}}},");
        }

        // Absent
        if (queryDesc.Absent.Any())
        {
            sb.AppendLine($"{indent}Absent = new ComponentType[] {{");
            foreach (var componentType in queryDesc.Absent)
            {
                var typeName = componentType.GetManagedType().FullName;
                sb.AppendLine($"{indent}{indent}ComponentType.Exclude<{typeName}>(),");
            }
            sb.AppendLine($"{indent}}},");
        }

        // Present
        if (queryDesc.Present.Any())
        {
            sb.AppendLine($"{indent}Present = new ComponentType[] {{");
            foreach (var componentType in queryDesc.Present)
            {
                var typeName = componentType.GetManagedType().FullName;
                if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
                {
                    sb.AppendLine($"{indent}{indent}ComponentType.ReadWrite<{typeName}>(),");
                }
                else
                {
                    sb.AppendLine($"{indent}{indent}ComponentType.ReadOnly<{typeName}>(),");
                }
            }
            sb.AppendLine($"{indent}}},");
        }

        // Options
        if (queryDesc.Options != EntityQueryOptions.Default)
        {
            sb.Append($"{indent}Options = ");
            var values = Enum.GetValues(typeof(EntityQueryOptions));
            var on = values.Cast<EntityQueryOptions>()
                .Distinct() // IncludeDisabled and IncludeDisabledEntities use the same bit. don't list it twice.
                .Where(value => (queryDesc.Options & value) != 0)
                .Select(value => $"EntityQueryOptions.{value}")
                .ToList();
            sb.Append(string.Join(" | ", on));
            sb.AppendLine(",");
        }

        sb.AppendLine("});");
    }

}