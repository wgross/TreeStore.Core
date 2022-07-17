namespace TreeStore.Core.Capabilities;

public interface IRemoveItemProperty
{
    /// <summary>
    /// Returns custom parameters to be applied for the removal of item properties
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? RemoveItemPropertyParameters(string property) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Removes the given item properties
    /// </summary>
    public void RemoveItemProperty(CmdletProvider provider, string property);
}