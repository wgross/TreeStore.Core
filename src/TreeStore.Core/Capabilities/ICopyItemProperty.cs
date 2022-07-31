using TreeStore.Core.Nodes;

namespace TreeStore.Core.Capabilities;

public interface ICopyItemProperty
{
    /// <summary>
    /// Returns custom parameters to be applied for the copying of item properties in
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? CopyItemPropertyParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Copy the given item property
    /// </summary>
    public void CopyItemProperty(ICmdletProvider provider, ProviderNode sourceNode, string sourceProperty, string destinationProperty);
}