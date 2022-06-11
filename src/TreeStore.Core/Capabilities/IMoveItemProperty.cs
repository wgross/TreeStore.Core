using TreeStore.Core.Nodes;
using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface IMoveItemProperty
    {
        /// <summary>
        /// Returns custom parameters to be applied for the moving of item properties in
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        public object? MoveItemPropertyParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Move the given item property
        /// </summary>
        /// <param name="properties"></param>
        public void MoveItemProperty(ProviderNode sourceNode, string sourceProperty, string destinationProperty);
    }
}