using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IRenameItemProperty
    {
        /// <summary>
        /// Returns custom parameters to be applied for the renaming of item properties
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? RenameItemPropertyParameters(string sourceProperty, string destinationProperty) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Rename the given item properties
        /// </summary>
        /// <param name="properties"></param>
        void RenameItemProperty(string sourceProperty, string destinationProperty);
    }
}