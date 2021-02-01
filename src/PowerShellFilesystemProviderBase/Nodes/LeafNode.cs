using System;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public class LeafNode : ProviderNode
    {
        public LeafNode(string? name, object? underlying)
            : base(name, underlying)
        { }

        
    }
}