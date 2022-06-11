using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    /// <summary>
    /// Implement PowerShell 'Invoke-Item' command
    /// </summary>
    public interface IInvokeItem
    {
        /// <summary>
        /// Dynamic parameters presented to PowerShell 'Invoke-Item' command
        /// </summary>
        public object? InvokeItemParameters() => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Implements the items invocation
        /// </summary>
        public void InvokeItem();
    }
}