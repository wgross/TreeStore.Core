using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Capabilities;

public interface INewChildItem
{
    /// <summary>
    /// Returns custom parameters to be applied for the creation a new child node named <paramref name="childName"/>
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? NewChildItemParameters(string childName, string itemTypeName, object newItemValue) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Creates a new child named <paramref name="childName"/>
    /// </summary>    
    public ProviderNode? NewChildItem(CmdletProvider provider, string childName, string? itemTypeName, object? newItemValue);
}