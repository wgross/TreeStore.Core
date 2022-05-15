using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    /// <summary>
    /// Implement PowerShell 'Invoke-Item' command
    /// </summary>
    public interface IInvokeItem
    {
        /// <summary>
        /// Dynamic parameters presented to PowerShell 'Invoke-Item' command
        /// </summary>
        object? InvokeItemParameters() => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Implements the items invocation
        /// </summary>
        void InvokeItem();
    }
}