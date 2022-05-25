using System;

namespace TreeStore.Core.Nodes
{
    /// <summary>
    /// Represents a <see cref="ProviderNode"/> without child nodes.
    /// </summary>
    public sealed record LeafNode : ProviderNode
    {
        public LeafNode(string? name, IServiceProvider underlyingProperties)
            : base(name, underlyingProperties)
        { }
    }
}