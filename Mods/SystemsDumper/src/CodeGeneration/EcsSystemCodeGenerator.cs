using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Cpp2IL.Core.Extensions;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.SystemsDumper.CodeGeneration;

class EcsSystemCodeGenerator
{
    private World _world;
    private EcsSystemMetadata _system;
    private Il2CppSystem.Type _type;

    private int _spacesPerIndent;
    private string _indent;
    private string _newLine;

    public EcsSystemCodeGenerator(World world, EcsSystemMetadata system, int spacesPerIndent = 4, string newLine = "\n")
    {
        _world = world;
        _system = system;
        _type = system.Type;
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

    private string Indent(int indents, string multiLineString)
    {
        var indent = _indent.Repeat(indents);
        return indent + multiLineString.Replace(_newLine, $"{_newLine}{indent}");
    }

    public string CreateCSharpFileContents()
    {
        var sw = NewStringWriter();
        sw.Write(Indent(0, GenerateUsingLines()));
        sw.WriteLine();

        if (_type.Namespace is not null)
        {
            sw.Write(Indent(0, GenerateNamespaceLine()));
            sw.WriteLine();
            sw.WriteLine();
        }
        
        sw.Write(Indent(0, GenerateAttributeLines()));
        sw.Write(Indent(0, GenerateMainSignatureLines()));
        sw.WriteLine("{");
        sw.Write(Indent(1, GenerateEntityQueryDeclarationLines()));
        // todo: more things?
        sw.WriteLine();
        sw.WriteLine("}");
        return sw.ToString();
    }

    public string GenerateUsingLines()
    {
        var sw = NewStringWriter();
        var x = Using1();
        foreach (var usedNamespace in x)
        {
            sw.WriteLine($"using {usedNamespace};");
        }
        return sw.ToString();
    }

    private IEnumerable<string> Using1()
    {
        // todo: cache, maybe init at start
        var namespaces = new List<string>();
        if (_type.Namespace is null || !_type.Namespace.Equals("Unity.Entities"))
        {
            namespaces.Add("Unity.Entities");
        }
        return namespaces;
    }

    private string ShortTypeName(Il2CppSystem.Type type)
    {
        if (type.Namespace == "Unity.Entities")
        {
            return type.Name;
        }

        // Only shorten if its a unity type. This makes it clear what's standard, and what's from the game.
        // if (type.Namespace == _type.Namespace)
        // {
        //     return type.Name;
        // }

        return type.FullName;
    }

    public string GenerateNamespaceLine()
    {
        if (_type.Namespace is null)
        {
            return "";
        }

        return $"namespace {_type.Namespace};{_newLine}";
    }

    public string GenerateAttributeLines()
    {
        var sw = NewStringWriter();

        // todo: see if there's something we can do with TypeManager.GetSystemAttributes.
        // would be nice to have handling for every possible attribute.
        // (currently manually doing everything due to Il2cpp types not passable as generics)

        // todo: SpawnInWorld
        // todo: RequiresMatchingQueriesForUpdate

        foreach (var attr in _system.Attributes.UpdateInGroup)
        {
            sw.Write($"[UpdateInGroup(typeof({ShortTypeName(attr.GroupType)})");
            if (attr.OrderFirst)
            {
                sw.Write(", OrderFirst = true");
            }
            if (attr.OrderLast)
            {
                sw.Write(", OrderLast = true");
            }
            sw.WriteLine(")]");
        }

        foreach (var attr in _system.Attributes.UpdateBefore)
        {
            sw.WriteLine($"[UpdateBefore(typeof({ShortTypeName(attr.SystemType)}))]");
        }

        foreach (var attr in _system.Attributes.UpdateAfter)
        {
            sw.WriteLine($"[UpdateAfter(typeof({ShortTypeName(attr.SystemType)}))]");
        }

        foreach (var attr in _system.Attributes.CreateBefore)
        {
            sw.WriteLine($"[CreateBefore(typeof({ShortTypeName(attr.SystemType)}))]");
        }

        foreach (var attr in _system.Attributes.CreateAfter)
        {
            sw.WriteLine($"[CreateAfter(typeof({ShortTypeName(attr.SystemType)}))]");
        }

        foreach (var attr in _system.Attributes.DisableAutoCreation)
        {
            sw.WriteLine($"[DisableAutoCreation]");
        }

        // todo: BurstCompile

        return sw.ToString();
    }

    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/classes
    public string GenerateMainSignatureLines()
    {
        var type = _system.Type;

        var sw = NewStringWriter();
        sw.Write("public ");
        sw.Write($"{TypeString(type)} ");
        sw.Write($"{type.Name}");

        if (type.IsClass || type.GetInterfaces().Any())
        {
            sw.Write($" : {BaseTypeAndInterfaces()}");
        }

        sw.WriteLine();
        return sw.ToString();
    }

    // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/types
    public string TypeString(Il2CppSystem.Type type)
    {
        if (type.IsInterface)
        {
            return "interface";
        }
        if (type.IsClass)
        {
            return "class";
        }
        if (type.IsValueType)
        {
            if (type.IsEnum)
            {
                return "enum";
            }
            if (type.IsPrimitive)
            {
                // todo: handle this. not needed right now
                LogUtil.LogWarning($"failed to make TypeString for {type.FullName}");
                return "";
            }
            return "struct";
        }
        LogUtil.LogWarning($"failed to make TypeString for {type.FullName}");
        return "";
    }

    public string BaseTypeAndInterfaces()
    {
        var typeNames = new List<string>();

        if (_type.IsClass)
        {
            typeNames.Add(ShortTypeName(_type.BaseType));
        }

        foreach (var interfaceType in _type.GetInterfaces())
        {
            typeNames.Add(ShortTypeName(interfaceType));
        }

        return string.Join(", ", typeNames);
    }

    public string GenerateEntityQueryDeclarationLines()
    {
        var sw = NewStringWriter();
        foreach (var namedQuery in _system.NamedEntityQueries)
        {
            sw.WriteLine($"EntityQuery {namedQuery.Name};");
        }
        return sw.ToString();
    }

    public class Factory(int spacesPerIndent = 4, string newLine = "\n")
    {
        private int _spacesPerIndent = spacesPerIndent;
        private string _newLine = newLine;

        public EcsSystemCodeGenerator CodeGenerator(World world, EcsSystemMetadata system)
        {
            return new EcsSystemCodeGenerator(world, system, _spacesPerIndent, _newLine);
        }
    }

}