using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Il2CppInterop.Runtime;
using Unity.Collections;
using Unity.Entities;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Services;

// responsible for finding ecs systems with entity queries, for any given world
public class EcsSystemMetadataService
{
    private ManualLogSource Log;

    public EcsSystemMetadataService(ManualLogSource log)
    {
        Log = log;
    }

    public IEnumerable<EcsSystemMetadata> CollectSystemMetadata(World world)
    {
        var models = new List<EcsSystemMetadata>();
        var systemHandles = world.Unmanaged.GetAllSystems(Allocator.Temp);

        foreach (var systemHandle in systemHandles)
        {
            var systemType = world.Unmanaged.GetTypeOfSystem(systemHandle);
            var systemTypeIndex = TypeManager.GetSystemTypeIndex(systemType);
            var category = CategorizeSystem(systemTypeIndex);
            var model = new EcsSystemMetadata(systemType, systemHandle, systemTypeIndex, category);

            if (TryGetTypeIL(systemType, out var systemTypeIL))
            {
                model.TypeIL = systemTypeIL;
            }

            FillAttrributes(model, systemType);

            try
            {
                var queries = FindNamedEntityQueriesFromSystem(world, systemHandle, systemTypeIndex, systemType);
                model.NamedEntityQueries.AddRange(queries);
            }
            catch (Exception ex)
            {
                model.ExceptionFromQueryFinding = ex;
            }
            models.Add(model);
        }

        return models;
    }

    private EcsSystemCategory CategorizeSystem(SystemTypeIndex systemTypeIndex)
    {
        return systemTypeIndex.IsGroup ? EcsSystemCategory.Group : systemTypeIndex.IsManaged ? EcsSystemCategory.Base : EcsSystemCategory.Unmanaged;
    }

    private bool TryGetTypeIL(Il2CppSystem.Type systemType, out System.Type systemTypeIL)
    {
        systemTypeIL = null;
        try
        {
            systemTypeIL = Type.GetType(systemType.AssemblyQualifiedName);
            return true;
        }
        catch (Exception ex)
        {
            // todo: solve this
            // Every error comes from CastleBuilding.blahblahEvent systems.
            // 
            // System.TypeLoadException: GenericArguments[1], 'TCompareComponent',
            //   on 'ProjectM.EntityAddRemoveUpdateEvents`2+EventData[TComponent,TCompareComponent]'
            //   violates the constraint of type parameter 'TCompareComponent'.
            Log.LogWarning($"error getting System.Type for {systemType.AssemblyQualifiedName}:\n{ex}");
            Log.LogDebug($"  IsGenericTypeDefinition: {systemType.IsGenericTypeDefinition}");
            return false;
        }
    }

    private IEnumerable<NamedEntityQuery> FindNamedEntityQueriesFromSystem(World world, SystemHandle systemHandle, SystemTypeIndex systemTypeIndex, Il2CppSystem.Type systemType)
    {
        Type systemTypeIL;
        try
        {
            systemTypeIL = Type.GetType(systemType.AssemblyQualifiedName);
        }
        catch (Exception ex)
        {
            // todo: solve this
            // Every error comes from CastleBuilding.blahblahEvent systems.
            // 
            // System.TypeLoadException: GenericArguments[1], 'TCompareComponent',
            //   on 'ProjectM.EntityAddRemoveUpdateEvents`2+EventData[TComponent,TCompareComponent]'
            //   violates the constraint of type parameter 'TCompareComponent'.
            Log.LogWarning($"error getting System.Type for {systemType.AssemblyQualifiedName}:\n{ex}");
            Log.LogDebug($"  IsGenericTypeDefinition: {systemType.IsGenericTypeDefinition}");
            throw;
        }

        object systemInstance;
        if (systemTypeIndex.IsManaged)
        {
            systemInstance = GetExistingSystemManaged(world, systemTypeIL);
        }
        else
        {
            systemInstance = GetExistingSystemUnmanaged(world, systemTypeIL, systemHandle);
        }

        return FindNamedEntityQueriesFromSystem(world, systemTypeIL, systemInstance);
    }

    unsafe private IEnumerable<NamedEntityQuery> FindNamedEntityQueriesFromSystem(World world, Type type, object system)
    {
        var queries = new List<NamedEntityQuery>();

        var queryType = typeof(EntityQuery);
        var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        var fields = type.GetFields(bindingFlags).Where(f => f.FieldType == queryType);
        foreach (var field in fields)
        {
            var query = (EntityQuery)field.GetValue(system);
            var namedQuery = new NamedEntityQuery(field.Name, query);
            queries.Add(namedQuery);
        }

        var properties = type.GetProperties(bindingFlags).Where(p => p.PropertyType == queryType);
        foreach (var property in properties)
        {
            var query = (EntityQuery)property.GetValue(system);
            var namedQuery = new NamedEntityQuery(property.Name, query);
            queries.Add(namedQuery);
        }
        
        return queries;
    }

    private void FillAttrributes(EcsSystemMetadata system, Il2CppSystem.Type systemType)
    {
        // Enum TypeManager.SystemAttributeKind

        foreach (var attr in systemType.GetCustomAttributes(Il2CppType.Of<UpdateInGroupAttribute>(), true))
        {
            var attrIL = (UpdateInGroupAttribute)attr.Cast<UpdateInGroupAttribute>();
            system.Attributes.UpdateInGroup.Add(attrIL);
        }

        foreach (var attr in systemType.GetCustomAttributes(Il2CppType.Of<UpdateBeforeAttribute>(), true))
        {
            var attrIL = (UpdateBeforeAttribute)attr.Cast<UpdateBeforeAttribute>();
            system.Attributes.UpdateBefore.Add(attrIL);
        }

        foreach (var attr in systemType.GetCustomAttributes(Il2CppType.Of<UpdateAfterAttribute>(), true))
        {
            var attrIL = (UpdateAfterAttribute)attr.Cast<UpdateAfterAttribute>();
            system.Attributes.UpdateAfter.Add(attrIL);
        }

        foreach (var attr in systemType.GetCustomAttributes(Il2CppType.Of<CreateBeforeAttribute>(), true))
        {
            var attrIL = (CreateBeforeAttribute)attr.Cast<CreateBeforeAttribute>();
            system.Attributes.CreateBefore.Add(attrIL);
        }

        foreach (var attr in systemType.GetCustomAttributes(Il2CppType.Of<CreateAfterAttribute>(), true))
        {
            var attrIL = (CreateAfterAttribute)attr.Cast<CreateAfterAttribute>();
            system.Attributes.CreateAfter.Add(attrIL);
        }

        foreach (var attr in systemType.GetCustomAttributes(Il2CppType.Of<DisableAutoCreationAttribute>(), true))
        {
            var attrIL = (DisableAutoCreationAttribute)attr.Cast<DisableAutoCreationAttribute>();
            system.Attributes.DisableAutoCreation.Add(attrIL);
        }
        
        foreach (var attr in systemType.GetCustomAttributes(Il2CppType.Of<RequireMatchingQueriesForUpdateAttribute>(), true))
        {
            var attrIL = (RequireMatchingQueriesForUpdateAttribute)attr.Cast<RequireMatchingQueriesForUpdateAttribute>();
            system.Attributes.RequireMatchingQueriesForUpdateAttribute.Add(attrIL);
        }
    }

    private ComponentSystemBase GetExistingSystemManaged(World world, Type systemType)
    {
        MethodInfo method = world.GetType().GetMethod("GetExistingSystemManaged", new Type[] { });
        MethodInfo generic = method.MakeGenericMethod(systemType);
        return (ComponentSystemBase)generic.Invoke(world, new object[] { });
    }

    private unsafe object GetExistingSystemUnmanaged(World world, Type systemType, SystemHandle systemHandle)
    {
        var systemState = world.Unmanaged.ResolveSystemState(systemHandle);
        return UnsafeUtil.DynamicDereference(systemState->m_SystemPtr, systemType);
    }

}