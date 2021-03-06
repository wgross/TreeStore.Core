using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface INewItemProperty
    {
        /// <summary>
        /// Returns custom parameters to be applied for the setting ofitem properties in
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? NewItemPropertyParameters(string propertyName, string? propertyTypeName, object? value) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// New the given item properties
        /// </summary>
        /// <param name="properties"></param>
        void NewItemProperty(string propertyName, string? propertyTypeName, object? value);
    }
}