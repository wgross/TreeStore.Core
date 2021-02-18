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
        /// Creata a noew child item as a copy of an existing child item from the same provider.
        /// </summary>
        /// <param name="childName"></param>
        /// <param name="itemTypeName"></param>
        /// <param name="newItemValue"></param>
        void NewChildItemAsCopy(string childName, object newItemValue);
    }
}