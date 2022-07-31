namespace TreeStore.Core.Nodes;

public sealed record RootNode : ContainerNode
{
    public RootNode(ICmdletProvider provider, IServiceProvider data)
        : base(provider, name: string.Empty, data)
    {
    }
}