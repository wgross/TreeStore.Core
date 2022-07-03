using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Capabilities;

public interface IMoveItemProperty
{
    /// <summary>
    /// Returns custom parameters to be applied for the moving of item properties in
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? MoveItemPropertyParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Move the given item property
    /// </summary>
    public void MoveItemProperty(CmdletProvider provider, ProviderNode sourceNode, string sourceProperty, string destinationProperty);
}