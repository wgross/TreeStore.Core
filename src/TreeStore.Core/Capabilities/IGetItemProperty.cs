using System.Collections.Generic;
using System.Management.Automation;

namespace TreeStore.Core.Capabilities;

public interface IGetItemProperty
{
    /// <summary>
    /// Returns custom parameters to be applied for the clearing an the item property name <paramref name="properties"/>
    /// </summary>
    /// <param name="properties"></param>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? GetItemPropertyParameters(IEnumerable<string>? properties) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Removes the value from an item property.
    /// </summary>
    /// <param name="properties"></param>
    public PSObject GetItemProperty(IEnumerable<string>? properties);
}