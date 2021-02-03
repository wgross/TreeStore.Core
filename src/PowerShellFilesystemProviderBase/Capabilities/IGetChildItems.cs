using PowerShellFilesystemProviderBase.Nodes;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IGetChildItems : IItemContainer
    {
        object? GetChildItemParameters() => new RuntimeDefinedParameterDictionary();

        IEnumerable<ProviderNode> GetChildItems();
    }
}