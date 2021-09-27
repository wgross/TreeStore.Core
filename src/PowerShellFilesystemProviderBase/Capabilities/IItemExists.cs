using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IItemExists
    {
        object? ItemExistsParameters() => new RuntimeDefinedParameterDictionary();

        bool ItemExists() => true;
    }
}