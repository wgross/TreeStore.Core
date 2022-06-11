using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface IItemExists
    {
        public object? ItemExistsParameters() => new RuntimeDefinedParameterDictionary();

        public bool ItemExists() => true;
    }
}