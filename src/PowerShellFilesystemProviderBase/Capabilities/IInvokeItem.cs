using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IInvokeItem
    {
        object? InvokeItemParameters() => new RuntimeDefinedParameterDictionary();

        void InvokeItem();
    }
}