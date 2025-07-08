using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using cheesasaurus.VRisingMods.SystemsDumper.Models;
using Il2CppInterop.Runtime;
using ProjectM;
using Unity.Entities;
using Unity.Physics.Systems;
using VRisingMods.Core.Utilities;

namespace cheesasaurus.VRisingMods.SystemsDumper;

class EcsSystemDumper
{
    private int _spacesPerIndent;

    public EcsSystemDumper(int spacesPerIndent = 4)
    {
        _spacesPerIndent = spacesPerIndent;
    }

    public string CreateDumpString(EcsSystemHierarchy systemHierarchy)
    {
        var sb = new StringBuilder();
        var sectionSeparator = "\n----------------------------------------\n\n";

        // Header
        sb.AppendLine($"Information about ECS Systems in world: {systemHierarchy.World.Name}");

        // Counts section
        sb.Append(sectionSeparator);
        AppendSectionCounts(sb, systemHierarchy);

        // Known Unknowns section
        if (systemHierarchy.Counts.Unknown > 0 && systemHierarchy.KnownUnknowns.AreKnown())
        {
            sb.Append(sectionSeparator);
            AppendSectionKnownUnknowns(sb, systemHierarchy);
        }

        // Multiple Parents section
        if (systemHierarchy.FindNodesWithMultipleParents().Any())
        {
            sb.Append(sectionSeparator);
            AppendSectionMultipleParents(sb, systemHierarchy);
        }

        // Update Hierarchy section
        if (systemHierarchy.RootNodesUnordered.Any())
        {
            sb.Append(sectionSeparator);
            AppendSectionUpdateHierarchy(sb, systemHierarchy);
        }

        return sb.ToString();
    }

    private void AppendSectionCounts(StringBuilder sb, EcsSystemHierarchy systemHierarchy)
    {
        var counts = systemHierarchy.Counts;
        sb.AppendLine($"[Counts]");
        sb.AppendLine();
        sb.AppendLine($"ComponentSystemGroup: {counts.Group}");
        sb.AppendLine($"ComponentSystemBase (excluding group instances): {counts.Base}");
        sb.AppendLine($"ISystem: {counts.Unmanaged}");
        sb.AppendLine($"<unknown system type>: {counts.Unknown}");
    }

    private void AppendSectionKnownUnknowns(StringBuilder sb, EcsSystemHierarchy systemHierarchy)
    {
        var knownUnknowns = systemHierarchy.KnownUnknowns;
        var singleIndent = new String(' ', _spacesPerIndent);
        sb.AppendLine($"[Known Unknowns]");
        sb.AppendLine();
        sb.AppendLine($"Potential reasons for <unknown system type>.");
        sb.AppendLine();
        if (knownUnknowns.SystemNotFoundInWorld.Any())
        {
            sb.AppendLine($"Issue: SystemHandle found in a group, but no corresponding system in the world");
            foreach (var type in knownUnknowns.SystemNotFoundInWorld)
            {
                sb.Append(singleIndent);
                sb.Append($"{type}");
                sb.AppendLine();
            }
        }
    }

    private void AppendSectionMultipleParents(StringBuilder sb, EcsSystemHierarchy systemHierarchy)
    {
        var nodesWithMultipleParents = systemHierarchy.FindNodesWithMultipleParents();
        var singleIndent = new String(' ', _spacesPerIndent);
        sb.AppendLine($"[Systems in multiple groups]");
        sb.AppendLine();
        sb.AppendLine($"This probably shouldn't happen!");
        sb.AppendLine();
        foreach (var node in nodesWithMultipleParents)
        {
            sb.AppendLine($"{SystemTypeDescription(node)} - belongs to {node.Parents.Count} groups");
            foreach (var parent in node.Parents)
            {
                sb.Append(singleIndent);
                sb.Append($"{SystemTypeDescription(parent)}");
                sb.AppendLine();
            }
        }
    }

    private void AppendSectionUpdateHierarchy(StringBuilder sb, EcsSystemHierarchy systemHierarchy)
    {
        sb.AppendLine($"[Update Hierarchy]");
        sb.AppendLine();
        sb.AppendLine($"The ordering at root level is arbitrary, but everything within a group is in update order for that group.");
        sb.AppendLine();
        foreach (var node in systemHierarchy.RootNodesUnordered)
        {
            AppendTreeNode(sb, node, 0);
        }
    }

    private void AppendTreeNode(StringBuilder sb, EcsSystemTreeNode node, int depth)
    {
        var leftPadding = new String(' ', _spacesPerIndent * depth);
        sb.Append(leftPadding);
        sb.Append(SystemDescription(node));
        sb.AppendLine();
        foreach (var childNode in node.ChildrenOrderedForUpdate)
        {
            AppendTreeNode(sb, childNode, depth + 1);
        }
    }

    internal static string SystemDescription(EcsSystemTreeNode node)
    {
        IList<String> parts = new List<String>();
        parts.Add(SystemTypeDescription(node));
        if (node.Category.Equals(EcsSystemCategory.Group))
        {
            parts.Add($"{node.CountDescendants()} descendants");
            parts.Add($"{node.ChildrenOrderedForUpdate.Count} children");
        }
        if (node.Parents.Count > 1)
        {
            parts.Add($"{node.Parents.Count} parents");
        }
        return String.Join(" | ", parts);
    }

    internal static string SystemTypeDescription(EcsSystemTreeNode node)
    {
        switch (node.Category)
        {
            case EcsSystemCategory.Group:
                return $"{node.Type.FullName} (ComponentSystemGroup)";
            case EcsSystemCategory.Base:
                return $"{node.Type.FullName} (ComponentSystemBase)";
            case EcsSystemCategory.Unmanaged:
                return $"{node.Type.FullName} (ISystem)";
            default:
            case EcsSystemCategory.Unknown:
                return DescribeUnknownSystem(node);
        }
    }

    internal static string DescribeUnknownSystem(EcsSystemTreeNode node)
    {
        if (TryGetTypeForMissedSystem(node.SystemHandle, out var systemType))
        {
            return $"{systemType.FullName} (system was unknown but later found)";
        }
        return "<unknown system type>";
    }

    internal static bool TryGetTypeForMissedSystem(SystemHandle systemHandle, out Il2CppSystem.Type systemType)
    {
        systemType = null;
        var serverWorld = WorldUtility.FindServerWorld();

        if (systemHandle.Equals(SystemHandle.Null))
        {
            return false;
        }
        if (!serverWorld.Unmanaged.IsSystemValid(systemHandle))
        {
            return false;
        }
        
        systemType = serverWorld.Unmanaged.GetTypeOfSystem(systemHandle);
        if (systemType is null)
        {
            return false;
        }

        var systemTypeIndex = TypeManager.GetSystemTypeIndex(systemType);
        var existsInList = false;
        foreach (var otherSystemTypeIndex in TypeManager.GetSystemTypeIndices())
        {
            if (systemTypeIndex.Equals(otherSystemTypeIndex))
            {
                existsInList = true;
                break;
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine($"Found missed system {systemType.FullName}");
        sb.AppendLine($"  SystemTypeIndex.Index: {systemTypeIndex.Index} (retrieved from TypeManager.GetSystemTypeIndex)");
        sb.AppendLine($"  is SystemTypeIndex in TypeManager.GetSystemTypeIndices: {existsInList}");
        LogUtil.LogDebug(sb.ToString());

        return true;
    }

}