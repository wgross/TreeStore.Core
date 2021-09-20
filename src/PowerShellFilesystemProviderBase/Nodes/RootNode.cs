using System;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public sealed record RootNode : ContainerNode
    {
        public RootNode(IServiceProvider data)
            : base(name: string.Empty, data)
        {
        }
    }
}