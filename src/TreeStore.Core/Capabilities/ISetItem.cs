﻿namespace TreeStore.Core.Capabilities;

/// <summary>
/// Sets the items value
/// </summary>
public interface ISetItem
{
    /// <summary>
    /// Provide dynamic parameters to PwerShells 'Set-Item' command.
    /// </summary>
    public object? SetItemParameters() => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Implements setting the items value.
    /// </summary>
    public void SetItem(ICmdletProvider provider, object? value);
}