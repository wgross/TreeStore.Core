using TreeStore.Core.Nodes;
using System;

namespace TreeStore.Core.Capabilities
{
    /// <summary>
    /// Marker interface to indicate that a <see cref="ProviderNode"/> is a container for other
    /// <see cref="ProviderNode"/>. Th interfcae allows the <see cref="PowerShellFileSystemProviderBase"/> to travers a path.
    /// </summary>
    public interface IItemContainer
    {
        /// <summary>
        /// This method is used during path traveesal to resolve a path item name
        /// to a <see cref="ProviderNode"/>.
        /// </summary>
        /// <param name="name">name of the node to retrieve</param>
        /// <returns></returns>
        //[Obsolete("For now..")]
        //(bool exists, ProviderNode? node) TryGetChildNode(string name);
    }
}