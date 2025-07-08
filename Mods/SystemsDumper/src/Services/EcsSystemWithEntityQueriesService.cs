using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx.Logging;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Unity.Collections;
using Unity.Entities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Services;

// responsible for finding ecs systems with entity queries, for any given world
public class EcsSystemWithEntityQueriesService
{
    private ManualLogSource Log;

    public EcsSystemWithEntityQueriesService(ManualLogSource log)
    {
        Log = log;
    }

    public IEnumerable<EcsSystemWithEntityQueries> FindSystemsWithEntityQueries(World world)
    {
        var models = new List<EcsSystemWithEntityQueries>();
        var systemHandles = world.Unmanaged.GetAllSystems(Allocator.Temp);

        foreach (var systemHandle in systemHandles)
        {
            var systemType = world.Unmanaged.GetTypeOfSystem(systemHandle);
            var systemTypeIndex = TypeManager.GetSystemTypeIndex(systemType);
            var category = CategorizeSystem(systemTypeIndex);
            var model = new EcsSystemWithEntityQueries(category, systemHandle, systemType);
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
            //Log.LogDebug($"  IsGenericType: {systemType.IsGenericType}");
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

        return FindNamedEntityQueriesFromSystem(systemTypeIL, systemInstance);
    }

    private IEnumerable<NamedEntityQuery> FindNamedEntityQueriesFromSystem(Type type, object system)
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

    private ComponentSystemBase GetExistingSystemManaged(World world, Type systemType)
    {
        MethodInfo method = world.GetType().GetMethod("GetExistingSystemManaged", new Type[] { });
        MethodInfo generic = method.MakeGenericMethod(systemType);
        return (ComponentSystemBase)generic.Invoke(world, new object[] { });
    }

    private object GetExistingSystemUnmanaged(World world, Type systemType, SystemHandle systemHandle)
    {
        MethodInfo method = world.Unmanaged.GetType().GetMethod("GetUnsafeSystemRef");
        MethodInfo generic = method.MakeGenericMethod(systemType);
        return generic.Invoke(world.Unmanaged, new object[] { systemHandle });
    }

}