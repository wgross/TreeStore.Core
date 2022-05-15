using System;

namespace TreeStore.Core.Nodes
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