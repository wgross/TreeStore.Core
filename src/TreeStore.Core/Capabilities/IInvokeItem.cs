using System.Management.Automation;
using System.Management.Automation.Provider;

namespace TreeStore.Core.Capabilities;

/// <summary>
/// Implement PowerShell 'Invoke-Item' command
/// </summary>
public interface IInvokeItem
{
    /// <summary>
    /// Dynamic parameters presented to PowerShell 'Invoke-Item' command
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? InvokeItemParameters(CmdletProvider provider) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Implements the items invocation
    /// </summary>
    public void InvokeItem(CmdletProvider provider);
}