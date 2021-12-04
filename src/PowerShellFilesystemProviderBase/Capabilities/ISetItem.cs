using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface ISetItem
    {
        object? SetItemParameters() => new RuntimeDefinedParameterDictionary();

        void SetItem(object? value);
    }
}