using System;
using System.Collections.Generic;
using Unity.Entities;

namespace SystemExplorationPoC;

class SystemsTreeNode
{
    public SystemCategory Category;
    public Type Type;
    public SystemHandle SystemHandle;
    public ComponentSystemBase Instance;
    public IList<SystemsTreeNode> Children { get; set; }
    public SystemsTreeNode Parent;

    // everything else for serialization only
    public String CategoryStr { get => Category.ToString(); }
    public String TypeStr { get => Type?.ToString(); }


}