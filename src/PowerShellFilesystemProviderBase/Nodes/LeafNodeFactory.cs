using System.Collections.Generic;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public class LeafNodeFactory
    {
        /// <summary>
        /// Creates always a leaf node. Event if the <paramref name="underlyingProperties"/> implements
        /// <see cref="Capabilities.IItemContainer"/> or <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="underlyingProperties">bag of properties associated with the leaf node</param>
        /// <returns></returns>
        public static LeafNode Create(string? name, object underlyingProperties) => new LeafNode(name, underlyingProperties);
    }
}