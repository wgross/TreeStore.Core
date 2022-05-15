using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    /// <summary>
    /// Sets the items value
    /// </summary>
    public interface ISetItem
    {
        /// <summary>
        /// Provide dynamic parameters to PwerShells 'Set-Item' command.
        /// </summary>
        /// <returns></returns>
        object? SetItemParameters() => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Implements setting the items value.
        /// </summary>
        void SetItem(object? value);
    }
}