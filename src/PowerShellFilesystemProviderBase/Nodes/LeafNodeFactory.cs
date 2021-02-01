using System.Collections.Generic;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public class LeafNodeFactory
    {
        /// <summary>
        /// Creates always a leaf node. Event if the <paramref name="underlying"/> implements
        /// <see cref="Capabilities.IItemContainer"/> or <see cref="IDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="underlying"></param>
        /// <returns></returns>
        public static LeafNode Create(string? name, object underlying) => new LeafNode(name, underlying);
    }
}