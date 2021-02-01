using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IGetItem
    {
        object? GetItemParameters() => new RuntimeDefinedParameterDictionary();

        PSObject? GetItem();
    }
}