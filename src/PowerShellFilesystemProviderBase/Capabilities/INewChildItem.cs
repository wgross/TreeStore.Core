using PowerShellFilesystemProviderBase.Nodes;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface INewChildItem
    {
        /// <summary>
        /// Returns custom parameters to be applied for the creation a new child node named <paramref name="childName"/>
        /// </summary>
        /// <param name="childName"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? NewChildItemParameters(string childName, string itemTypeName, object newItemValue) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Creates a new child named <paramref name="childName"/>
        /// </summary>
        /// <param name="childName"></param>
        /// <returns>the resultion <see cref="ProviderNode"/> or null</returns>
        ProviderNode? NewChildItem(string childName, string itemTypeName, object newItemValue);
    }
}