﻿using System.Management.Automation;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Capabilities;

public interface ICopyChildItem
{
    /// <summary>
    /// Returns custom parameters to be applied for the copying a child node named <paramref name="childName"/> to a destination.
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? CopyChildItemParameters(string childName, string destination, bool recurse) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Creates a new child node at <paramref name="destination"/> from the given <paramref name="nodeToCopy"/>
    /// </summary>
    public ProviderNode? CopyChildItem(ProviderNode nodeToCopy, string[] destination);
}