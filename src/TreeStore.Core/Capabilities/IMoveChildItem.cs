using TreeStore.Core.Nodes;
using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface IMoveChildItem
    {
        /// <summary>
        /// Returns custom parameters to be applied for moving a child node named <paramref name="childName"/> to a destination.
        /// </summary>
        public object? MoveChildItemParameters(string childName, string destination) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Move the node <paramref name="nodeToMove"/> as a child under 'this' node.
        /// </summary>
        public ProviderNode? MoveChildItem(ContainerNode parentOfNodeToMove, ProviderNode nodeToMove, string[] destination);
    }
}