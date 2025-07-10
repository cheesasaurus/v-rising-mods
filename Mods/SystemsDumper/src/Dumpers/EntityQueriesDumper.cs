using System;
using System.Collections.Generic;
using System.Text;
using cheesasaurus.VRisingMods.SystemsDumper.CodeGeneration;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Cpp2IL.Core.Extensions;
using Unity.Entities;

namespace cheesasaurus.VRisingMods.SystemsDumper.Dumpers;

class EntityQueriesDumper
{
    private EntityQueryCodeGenerator _queryCodeGen;    

    public EntityQueriesDumper(EntityQueryCodeGenerator queryCodeGen)
    {
        _queryCodeGen = queryCodeGen;
    }

    public string ListAllQueries(World world, IEnumerable<EcsSystemMetadata> systems)
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

    private void AppendSystemSection(StringBuilder sb, EcsSystemMetadata system)
    {
        var subsectionSeparator = "-------------------------------------\n\n";

        sb.AppendLine($"{system.Type.FullName} ({DescribeCategory(system)})");
        sb.AppendLine();
        sb.AppendLine($"Entity Queries: {system.NamedEntityQueries.Count}");
        sb.AppendLine();
        sb.AppendLine(subsectionSeparator);

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

    private string DescribeCategory(EcsSystemMetadata system)
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

    private bool ValidateAndAppendProblems(StringBuilder sb, NamedEntityQuery namedQuery, EcsSystemMetadata system)
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

    // EntityQueryDesc is not the way unity recommends, but it is a hell of a lot more convenient with our il2cpp restrictions.
    unsafe private void AppendQueryDescSnippet(StringBuilder sb, NamedEntityQuery namedQuery, EcsSystemMetadata system)
    {
        sb.AppendLine(_queryCodeGen.CreateQueryFromQueryDescSnippet(namedQuery, system));
    }

}