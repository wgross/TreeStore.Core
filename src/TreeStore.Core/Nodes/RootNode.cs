using System;

namespace TreeStore.Core.Nodes; 

public sealed record RootNode : ContainerNode
{
    public RootNode(IServiceProvider data)
        : base(name: string.Empty, data)
    {
    }
}