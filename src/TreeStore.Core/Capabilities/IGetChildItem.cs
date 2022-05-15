using TreeStore.Core.Nodes;
using System.Collections.Generic;
using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    /// <summary>
    /// Access a <sse cref="ProviderNode"/> child nodes.
    /// </summary>
    public interface IGetChildItem : IItemContainer
    {
        /// <summary>
        /// Dynamic parameters provided to PowerShells command 'Get-ChildItem'
        /// </summary>
        object? GetChildItemParameters(string path, bool recurse) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Determines if this node has child nodes.
        /// </summary>
        bool HasChildItems();

        /// <summary>
        /// Enumerates all child <see cref="ProviderNode"/>
        /// </summary>
        IEnumerable<ProviderNode> GetChildItems();
    }
}