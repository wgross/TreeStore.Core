using PowerShellFilesystemProviderBase.Nodes;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IGetChildItems : IItemContainer
    {
        object? GetChildItemParameters(string path, bool recurse) => new RuntimeDefinedParameterDictionary();

        bool HasChildItems();

        IEnumerable<ProviderNode> GetChildItems();
    }
}