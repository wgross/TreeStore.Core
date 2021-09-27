using PowerShellFilesystemProviderBase.Nodes;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IMoveChildItem
    {
        /// <summary>
        /// Returns custom parameters to be applied for the copying achild node named <paramref name="childName"/> to a destination.
        /// </summary>
        object? MoveChildItemParameters(string childName, string destination) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Creates a new child item from an existing node.
        /// </summary>
        void MoveChildItem(ContainerNode parentOfNode, ProviderNode nodeToMove, string[] destination);
    }
}