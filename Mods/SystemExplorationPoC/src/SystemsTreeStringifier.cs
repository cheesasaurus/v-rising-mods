using System;
using System.Collections.Generic;
using System.Text;
using Cpp2IL.Core.Extensions;
using Unity.Entities;

namespace SystemExplorationPoC;

public class SystemsTreeStringifier
{
    private int _spacesPerTab = 2;

    public string CreateString(IList<SystemsTreeNode> treeRoots, World world)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Systems in world {world.Name}");
        foreach (var node in treeRoots)
        {
            AddNodeToBuilder(sb, node, 0);
        }
        return sb.ToString();
    }

    private void AddNodeToBuilder(StringBuilder sb, SystemsTreeNode node, int depth)
    {
        var leftPadding = new String(' ', _spacesPerTab * depth);
        sb.Append(leftPadding);
        sb.Append(SystemDescription(node));
        sb.AppendLine();
        foreach (var childNode in node.Children) {
            AddNodeToBuilder(sb, childNode, depth + 1);
        }
    }

    private string SystemDescription(SystemsTreeNode node)
    {
        switch (node.Category)
        {
            case SystemCategory.Group:
                return $"{node.Type} (ComponentSystemGroup) - {node.Children.Count} children";
            case SystemCategory.Base:
                return $"{node.Type} (ComponentSystemBase)";
            case SystemCategory.Unmanaged:
                return $"{node.Type} (ISystem)";
            default:
            case SystemCategory.Unknown:
                return "[unknown system]";
        }
    }
}
