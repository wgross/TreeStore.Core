using PowerShellFilesystemProviderBase.Nodes;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IMoveItemProperty
    {
        /// <summary>
        /// Returns custom parameters to be applied for the moving of item properties in
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? MoveItemPropertyParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Move the given item property
        /// </summary>
        /// <param name="properties"></param>
        void MoveItemProperty(ProviderNode sourceNode, string sourceProperty, string destinationProperty);
    }
}