namespace TreeStore.Core.Capabilities;

public interface IItemExists
{
    public object? ItemExistsParameters(ICmdletProvider provider) => new RuntimeDefinedParameterDictionary();

    public bool ItemExists(ICmdletProvider provider) => true;
}