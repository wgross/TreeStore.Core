using TreeStore.Core.Nodes;
using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface ICopyItemProperty
    {
        /// <summary>
        /// Returns custom parameters to be applied for the copying of item properties in
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        public object? CopyItemPropertyParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Copy the given item property
        /// </summary>
        /// <param name="properties"></param>
        public void CopyItemProperty(ProviderNode sourceNode, string sourcePropertyName, string destinationPropertyName);
    }
}