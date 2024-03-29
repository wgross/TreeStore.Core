﻿namespace TreeStore.Core.Capabilities;

public interface IClearItemProperty
{
    /// <summary>
    /// Returns custom parameters to be applied for the clearing an the item property name <paramref name="propertiesToClear"/>
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? ClearItemPropertyParameters(IEnumerable<string> propertiesToClear) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Removes the value from an item property.
    /// </summary>
    public void ClearItemProperty(ICmdletProvider provider, IEnumerable<string> propertiesToClear);
}