﻿namespace TreeStore.Core.Nodes;

/// <summary>
/// Represents a <see cref="ProviderNode"/> without child nodes.
/// </summary>
public sealed record LeafNode : ProviderNode
{
    public LeafNode(ICmdletProvider provider, string? name, IServiceProvider underlyingProperties)
        : base(provider, name, underlyingProperties)
    { }
}