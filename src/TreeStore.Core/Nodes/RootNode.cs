namespace TreeStore.Core.Nodes;

public sealed record RootNode : ContainerNode
{
    public RootNode(CmdletProvider provider, IServiceProvider data)
        : base(provider, name: string.Empty, data)
    {
    }
}