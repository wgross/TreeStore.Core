using System;

namespace PowerShellFilesystemProviderBase.Nodes
{
    /// <summary>
    /// Representa a <see cref="ProvioderNode"/> with child nodes.
    /// </summary>
    public sealed record LeafNode : ProviderNode
    {
        public LeafNode(string? name, IServiceProvider underlyingProperties)
            : base(name, underlyingProperties)
        { }
    }
}