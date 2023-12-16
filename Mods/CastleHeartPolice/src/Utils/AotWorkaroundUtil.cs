using Il2CppInterop.Runtime;
using Unity.Entities;

namespace CastleHeartPolice.Utils;

public static class AotWorkaroundUtil {
    public static int TypeIndex<T>() {
        var componentType = new ComponentType(Il2CppType.Of<T>(), ComponentType.AccessMode.ReadWrite);
        return componentType.TypeIndex;
    }
}