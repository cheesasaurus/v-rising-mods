using System.Linq;
using System.Text;
using Il2CppInterop.Runtime;

namespace cheesasaurus.VRisingMods.SystemsDumper.CodeGeneration;

public static class CodeGenUtil
{
    public static string FullTypeCode(System.Type type)
    {
        return FullTypeCode(Il2CppType.From(type));
    }

    public static string FullTypeCode(Il2CppSystem.Type type)
    {
        var sb = new StringBuilder();

        if (type.IsNested && !type.IsGenericType)
        {
            sb.Append(type.DeclaringType.FullName);
            sb.Append(".");
        }
        else if (type.Namespace is not null)
        {
            sb.Append($"{type.Namespace}.");
        }

        sb.Append(TypeCode(type));
        return sb.ToString();
    }

    public static string TypeCode(Il2CppSystem.Type type)
    {
        var sb = new StringBuilder();
        if (type.IsGenericType && type.IsNested)
        {
            var argTypes = type.GenericTypeArguments.Select(FullTypeCode);
            sb.Append(TypeNamePure(type.DeclaringType));
            sb.Append("<");
            sb.Append(string.Join(", ", argTypes));
            sb.Append(">");
            sb.Append(".");
        }
        sb.Append(TypeNamePure(type));

        if (type.IsGenericType && !type.IsNested)
        {
            var argTypes = type.GenericTypeArguments.Select(FullTypeCode);
            sb.Append("<");
            sb.Append(string.Join(", ", argTypes));
            sb.Append(">");
        }

        return sb.ToString();
    }

    private static string TypeNamePure(Il2CppSystem.Type type)
    {
        var name = type.Name;

        var backtickIndex = name.IndexOf("`");
        if (backtickIndex != -1)
        {
            return name.Substring(0, backtickIndex);
        }
        
        return name;
    }
    
}
