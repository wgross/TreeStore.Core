using PowerShellFilesystemProviderBase.Nodes;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface ICopyChildItem
    {
        /// <summary>
        /// Returns custom parameters to be applied for the copying achild node named <paramref name="childName"/> to a destination.
        /// </summary>
        /// <param name="childName"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? CopyChildItemParameters(string childName, string destination, bool recurse) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Creates a new child node at <paramref name="destination"/> from the given <paramref name="nodeToCopy"/>
        /// </summary>
        /// <param name="nodeToCopy"></param>
        /// <param name="destination"></param>
        ProviderNode CopyChildItem(ProviderNode nodeToCopy, string[] destination);
    }

    /// <summary>
    /// Extends an underlying of a <see cref="ContainerNode"/> to process a recursive copy of a given node.
    /// </summary>
    public interface ICopyChildItemRecursive : ICopyChildItem
    {
        /// <summary>
        /// Creates a new child node at <paramref name="destination"/> from the given <paramref name="nodeToCopy"/>
        /// as a recursive copy
        /// </summary>
        /// <param name="nodeToCopy"></param>
        /// <param name="destination"></param>
        ProviderNode CopyChildItemRecursive(ProviderNode nodeToCopy, string[] destination);
    }
}