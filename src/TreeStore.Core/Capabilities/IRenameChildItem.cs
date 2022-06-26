﻿using System.Management.Automation;

namespace TreeStore.Core.Capabilities;

public interface IRenameChildItem
{
    /// <summary>
    /// Returns custom parameters to be applied for the renaming of a child named <paramref name="childName"/>
    /// </summary>
    /// <param name="childName"></param>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? RenameChildItemParameters(string childName, string newName) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Renames a new child named <paramref name="childName"/> to <paramref name="newName"/>
    /// </summary>
    /// <param name="childName"></param>
    /// <returns>the resulting <see cref="ProviderNode"/> or null</returns>
    public void RenameChildItem(string childName, string newName);
}