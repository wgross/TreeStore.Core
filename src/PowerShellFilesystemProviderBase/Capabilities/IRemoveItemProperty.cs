using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IRemoveItemProperty
    {
        /// <summary>
        /// Returns custom parameters to be applied for the removal of item properties
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? RemoveItemPropertyParameters(string propertyName) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Removes the given item properties
        /// </summary>
        /// <param name="properties"></param>
        void RemoveItemProperty(string propertyName);
    }
}