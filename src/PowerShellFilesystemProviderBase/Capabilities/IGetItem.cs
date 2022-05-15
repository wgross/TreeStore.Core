using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    /// <summary>
    /// Get an item representation to write to the pipe
    /// </summary>
    public interface IGetItem
    {
        /// <summary>
        /// Dynamic parameter provided to PowerShells 'Get-Item' command.
        /// </summary>
        object? GetItemParameters() => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Creates a <see cref="PSObject"/> wrapping the implementing class of this interface in the <see cref="PowerShell"/> pipe.
        /// </summary>
        PSObject? GetItem();
    }
}