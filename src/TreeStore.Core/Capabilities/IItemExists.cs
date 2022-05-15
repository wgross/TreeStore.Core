using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface IItemExists
    {
        object? ItemExistsParameters() => new RuntimeDefinedParameterDictionary();

        bool ItemExists() => true;
    }
}