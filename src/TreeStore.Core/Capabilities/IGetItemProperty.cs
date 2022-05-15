using System.Collections.Generic;
using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface IGetItemProperty
    {
        /// <summary>
        /// Returns custom parameters to be applied for the clearing an the item property name <paramref name="propertyToGet"/>
        /// </summary>
        /// <param name="propertyToGet"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? GetItemPropertyParameters(IEnumerable<string>? propertyToGet) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Removes the value from an item property.
        /// </summary>
        /// <param name="propertyToGet"></param>
        PSObject GetItemProperty(IEnumerable<string>? propertyToGet);
    }
}