using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IClearItem
    {
        object? ClearItemParameters() => new RuntimeDefinedParameterDictionary();

        void ClearItem();
    }
}