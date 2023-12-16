using System;
using System.Runtime.InteropServices;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace VRisingMods.Core.Utilities;

// alternative methods to work around EntityManager exceptions like "Attempting to call method blahblah for which no ahead of time (AOT) code was generated."
public static class AotWorkaroundUtil {

    // alternative for Entitymanager.HasComponent
    public static bool HasComponent<T>(Entity entity) where T : struct  {
        return VWorld.Server.EntityManager.HasComponent(entity, ComponentType<T>());
    }

    // more convenient than Entitymanager.AddComponent
    public static bool AddComponent<T>(Entity entity) where T : struct  {
        return VWorld.Server.EntityManager.AddComponent(entity, ComponentType<T>());
    }

    // alternative for Entitymanager.AddComponentData
    public static void AddComponentData<T>(Entity entity, T componentData) where T : struct  {
       AddComponent<T>(entity);
       SetComponentData(entity, componentData);
    }

    // alternative for Entitymanager.RemoveComponent
    public static bool RemoveComponent<T>(Entity entity) where T : struct  {
        return VWorld.Server.EntityManager.RemoveComponent(entity, ComponentType<T>());
    }

    // alternative for EntityMManager.GetComponentData
    public unsafe static T GetComponentData<T>(Entity entity) where T : struct  {
        void* rawPointer = VWorld.Server.EntityManager.GetComponentDataRawRO(entity, ComponentTypeIndex<T>());
        return Marshal.PtrToStructure<T>(new System.IntPtr(rawPointer));
    }

    // alternative for EntityManager.SetComponentData
    public unsafe static void SetComponentData<T>(Entity entity, T componentData) where T : struct {
        var size = Marshal.SizeOf(componentData);
        //byte[] byteArray = new byte[size];
        var byteArray = StructureToByteArray(componentData);
        fixed (byte* data = byteArray) {
            //UnsafeUtility.CopyStructureToPtr(ref componentData, data);
            VWorld.Server.EntityManager.SetComponentDataRaw(entity, ComponentTypeIndex<T>(), data, size);
        }
    }

    private static ComponentType ComponentType<T>() {
        return new ComponentType(Il2CppType.Of<T>());
    }

    private static int ComponentTypeIndex<T>() {
        return ComponentType<T>().TypeIndex;
    }

    // https://stackoverflow.com/questions/3278827/how-to-convert-a-structure-to-a-byte-array-in-c
    private static byte[] StructureToByteArray<T>(T structure) where T : struct {
        int size = Marshal.SizeOf(structure);
        byte[] byteArray = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        try {
            Marshal.StructureToPtr(structure, ptr, true);
            Marshal.Copy(ptr, byteArray, 0, size);
        }
        finally {
            Marshal.FreeHGlobal(ptr);
        }
        return byteArray;
    }

}
