using System.Management.Automation;
using System.Management.Automation.Provider;

namespace TreeStore.Core.Capabilities;

public interface IItemExists
{
    public object? ItemExistsParameters(CmdletProvider provider) => new RuntimeDefinedParameterDictionary();

    public bool ItemExists(CmdletProvider provider) => true;
}