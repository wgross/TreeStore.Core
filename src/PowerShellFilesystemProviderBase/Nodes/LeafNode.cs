using System;

namespace PowerShellFilesystemProviderBase.Nodes
{
    /// <summary>
    /// Representa a <see cref="ProvioderNode"/> with child nodes.
    /// </summary>
    public class LeafNode : ProviderNode
    {
        public LeafNode(string? name, object? underlyingProperties)
            : base(name, underlyingProperties)
        { }

        
    }
}