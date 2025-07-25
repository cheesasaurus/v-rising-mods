using System.Reflection;

namespace VRisingMods.Core.Utilities;

unsafe public static class UnsafeUtil
{
    public unsafe static object DynamicDereference(void* ptr, System.Type type)
    {
        var ownType = typeof(UnsafeUtil);
        var bindingFlags = BindingFlags.NonPublic | BindingFlags.Static;
        MethodInfo method = ownType.GetMethod("Dereference", bindingFlags);
        MethodInfo genericMethod = method.MakeGenericMethod(type);
        var param0 = Pointer.Box(ptr, type.MakePointerType());
        var parameters = new[] { param0 };
        return genericMethod.Invoke(null, parameters);
    }

    private static T Dereference<T>(object boxedPtr)
    {
        return *(T*)Pointer.Unbox(boxedPtr);
    }

}