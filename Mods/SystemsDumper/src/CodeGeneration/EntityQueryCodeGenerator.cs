using System;
using System.IO;
using System.Linq;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Cpp2IL.Core.Extensions;
using Unity.Entities;
using static Unity.Entities.ComponentType;

namespace cheesasaurus.VRisingMods.SystemsDumper.CodeGeneration;

public class EntityQueryCodeGenerator
{
    private int _spacesPerIndent;
    private string _indent;
    private string _newLine;

    public EntityQueryCodeGenerator(int spacesPerIndent = 4, string newLine = "\n")
    {
        _spacesPerIndent = spacesPerIndent;
        _indent = " ".Repeat(spacesPerIndent);
        _newLine = newLine;
    }

    private StringWriter NewStringWriter()
    {
        return new StringWriter()
        {
            NewLine = _newLine,
        };
    }

    /// <summary>
    /// Creates an EntityQuery snippet like <c>var query = new EntityQueryBuilder(Allocator.Temp...</c>
    /// </summary>
    /// <remarks>
    /// The snippet created here sadly won't work.
    /// We have to pass the il2cpp types, which cannot be done with generics.
    ///
    /// There are methods we could use, such as:
    /// <code>
    /// <![CDATA[
    ///     WithPresent(ComponentType* componentTypes, int count)
    /// ]]>
    /// </code>
    ///
    /// But we cannot reference things created inline - they have to point to a variable. eg:
    /// <code>
    /// <![CDATA[
    ///     var component = new ComponentType(Il2CppType.Of<ProjectM.Network.FromCharacter>(), ComponentType.AccessMode.ReadOnly);
    ///     var _EventQuery = new EntityQueryBuilder(Allocator.Temp)
    ///        .WithPresent(&component, 1)
    ///        .Build(entityManager)
    /// ]]>
    /// </code>
    ///  
    /// which when using lots of components with a variety of filters, makes it rather unreadable.
    /// The idea is to tell at a glance how the components are being used, and to also have a usable snippet.
    /// </remarks>
    public string Snippet_CreateQueryFrom_QueryBuilder(NamedEntityQuery namedQuery, EcsSystemMetadata system)
    {
        var queryDesc = namedQuery.Query.GetEntityQueryDesc();

        var sw = NewStringWriter();

        sw.WriteLine($"var {namedQuery.Name} = new EntityQueryBuilder(Allocator.Temp)");

        // All
        foreach (var componentType in queryDesc.All)
        {
            if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
            {
                sw.WriteLine($"{_indent}.WithAllRW<{componentType.GetManagedType().FullName}>()");
            }
            else
            {
                sw.WriteLine($"{_indent}.WithAll<{componentType.GetManagedType().FullName}>()");
            }
        }

        // Any
        foreach (var componentType in queryDesc.Any)
        {
            if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
            {
                sw.WriteLine($"{_indent}.WithAnyRW<{componentType.GetManagedType().FullName}>()");
            }
            else
            {
                sw.WriteLine($"{_indent}.WithAny<{componentType.GetManagedType().FullName}>()");
            }
        }

        // None
        foreach (var componentType in queryDesc.None)
        {
            sw.WriteLine($"{_indent}.WithNone<{componentType.GetManagedType().FullName}>()");
        }

        // Disabled
        foreach (var componentType in queryDesc.Disabled)
        {
            if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
            {
                sw.WriteLine($"{_indent}.WithDisabledRW<{componentType.GetManagedType().FullName}>()");
            }
            else
            {
                sw.WriteLine($"{_indent}.WithDisabled<{componentType.GetManagedType().FullName}>()");
            }
        }

        // Absent
        foreach (var componentType in queryDesc.Absent)
        {
            sw.WriteLine($"{_indent}.WithAbsent<{componentType.GetManagedType().FullName}>()");
        }

        // Present
        foreach (var componentType in queryDesc.Present)
        {
            if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
            {
                sw.WriteLine($"{_indent}.WithPresentRW<{componentType.GetManagedType().FullName}>()");
            }
            else
            {
                sw.WriteLine($"{_indent}.WithPresent<{componentType.GetManagedType().FullName}>()");
            }
        }

        // todo: options

        sw.WriteLine($"{_indent}.Build(entityManager);");
        return sw.ToString();
    }

    /// <summary>
    /// Creates an EntityQuery snippet like <c>var query = entityManager.CreateEntityQuery(new EntityQueryDesc...</c> 
    /// </summary>
    /// <remarks>
    /// EntityQueryDesc is not the way unity recommends, but it is a hell of a lot more readable with our il2cpp constraints.
    /// </remarks>
    unsafe public string Snippet_CreateQueryFrom_QueryDesc(NamedEntityQuery namedQuery, EcsSystemMetadata system)
    {
        var sw = NewStringWriter();
        sw.Write($"var {namedQuery.Name} = entityManager.CreateEntityQuery(");
        sw.Write(Snippet_QueryDesc(namedQuery, system));
        sw.WriteLine(");");
        return sw.ToString();
    }

    /// <summary>
    /// Creates an EntityQuery snippet like <c>var query = entityManager.CreateEntityQuery(new EntityQueryDesc...</c> 
    /// </summary>
    /// <remarks>
    /// EntityQueryDesc is not the way unity recommends, but it is a hell of a lot more readable with our il2cpp constraints.
    /// </remarks>
    unsafe public string Snippet_QueryDesc(NamedEntityQuery namedQuery, EcsSystemMetadata system)
    {
        var queryDesc = namedQuery.Query.GetEntityQueryDesc();

        var sw = NewStringWriter();
        sw.WriteLine($"new EntityQueryDesc()");
        sw.WriteLine("{");

        // All
        if (queryDesc.All.Any())
        {
            sw.WriteLine($"{_indent}All = new ComponentType[] {{");
            foreach (var componentType in queryDesc.All)
            {
                var typeName = componentType.GetManagedType().FullName;
                if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
                {
                    sw.WriteLine($"{_indent}{_indent}ComponentType.ReadWrite<{typeName}>(),");
                }
                else
                {
                    sw.WriteLine($"{_indent}{_indent}ComponentType.ReadOnly<{typeName}>(),");
                }
            }
            sw.WriteLine($"{_indent}}},");
        }

        // Any
        if (queryDesc.Any.Any())
        {
            sw.WriteLine($"{_indent}Any = new ComponentType[] {{");
            foreach (var componentType in queryDesc.Any)
            {
                var typeName = componentType.GetManagedType().FullName;
                if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
                {
                    sw.WriteLine($"{_indent}{_indent}ComponentType.ReadWrite<{typeName}>(),");
                }
                else
                {
                    sw.WriteLine($"{_indent}{_indent}ComponentType.ReadOnly<{typeName}>(),");
                }
            }
            sw.WriteLine($"{_indent}}},");
        }

        // None
        if (queryDesc.None.Any())
        {
            sw.WriteLine($"{_indent}None = new ComponentType[] {{");
            foreach (var componentType in queryDesc.None)
            {
                var typeName = componentType.GetManagedType().FullName;
                sw.WriteLine($"{_indent}{_indent}ComponentType.ReadOnly<{typeName}>(),");
            }
            sw.WriteLine($"{_indent}}},");
        }

        // Disabled
        if (queryDesc.Disabled.Any())
        {
            sw.WriteLine($"{_indent}Disabled = new ComponentType[] {{");
            foreach (var componentType in queryDesc.Disabled)
            {
                var typeName = componentType.GetManagedType().FullName;
                if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
                {
                    sw.WriteLine($"{_indent}{_indent}ComponentType.ReadWrite<{typeName}>(),");
                }
                else
                {
                    sw.WriteLine($"{_indent}{_indent}ComponentType.ReadOnly<{typeName}>(),");
                }
            }
            sw.WriteLine($"{_indent}}},");
        }

        // Absent
        if (queryDesc.Absent.Any())
        {
            sw.WriteLine($"{_indent}Absent = new ComponentType[] {{");
            foreach (var componentType in queryDesc.Absent)
            {
                var typeName = componentType.GetManagedType().FullName;
                sw.WriteLine($"{_indent}{_indent}ComponentType.Exclude<{typeName}>(),");
            }
            sw.WriteLine($"{_indent}}},");
        }

        // Present
        if (queryDesc.Present.Any())
        {
            sw.WriteLine($"{_indent}Present = new ComponentType[] {{");
            foreach (var componentType in queryDesc.Present)
            {
                var typeName = componentType.GetManagedType().FullName;
                if (componentType.AccessModeType.Equals(AccessMode.ReadWrite))
                {
                    sw.WriteLine($"{_indent}{_indent}ComponentType.ReadWrite<{typeName}>(),");
                }
                else
                {
                    sw.WriteLine($"{_indent}{_indent}ComponentType.ReadOnly<{typeName}>(),");
                }
            }
            sw.WriteLine($"{_indent}}},");
        }

        // Options
        if (queryDesc.Options != EntityQueryOptions.Default)
        {
            sw.Write($"{_indent}Options = ");
            var values = Enum.GetValues(typeof(EntityQueryOptions));
            var on = values.Cast<EntityQueryOptions>()
                .Distinct() // IncludeDisabled and IncludeDisabledEntities use the same bit. don't list it twice.
                .Where(value => (queryDesc.Options & value) != 0)
                .Select(value => $"EntityQueryOptions.{value}")
                .ToList();
            sw.Write(string.Join(" | ", on));
            sw.WriteLine(",");
        }

        sw.Write("}");
        return sw.ToString();
    }

}
