using PowerShellFilesystemProviderBase.Nodes;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IMoveChildItem
    {
        /// <summary>
        /// Returns custom parameters to be applied for the copying achild node named <paramref name="childName"/> to a destination.
        /// </summary>
        /// <param name="childName"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? MoveChildItemParameters(string childName, string destination, bool recurse) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Creates a new child item from an existing node.
        /// </summary>
        /// <param name="childName"></param>
        /// <param name="itemTypeName"></param>
        /// <param name="newItemValue"></param>
        void MoveChildItem(ContainerNode parentOfNode, ProviderNode nodeToMove, string[] destination);
    }
}