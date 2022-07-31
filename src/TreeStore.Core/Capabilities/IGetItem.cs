namespace TreeStore.Core.Capabilities;

/// <summary>
/// Get an item representation to write to the pipe
/// </summary>
public interface IGetItem
{
    /// <summary>
    /// Dynamic parameter provided to PowerShells 'Get-Item' command.
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? GetItemParameters() => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Creates a <see cref="PSObject"/> wrapping the implementing class of this interface in the <see cref="PowerShell"/> pipe.
    /// </summary>
    public PSObject GetItem(ICmdletProvider provider);
}