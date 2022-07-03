using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace TreeStore.Core.Capabilities;

public interface IGetItemProperty
{
    /// <summary>
    /// Returns custom parameters to be applied for the clearing an the item property name <paramref name="properties"/>
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? GetItemPropertyParameters(IEnumerable<string>? properties) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Removes the value from an item property.
    /// </summary>
    public PSObject GetItemProperty(CmdletProvider provider, IEnumerable<string>? properties);
}