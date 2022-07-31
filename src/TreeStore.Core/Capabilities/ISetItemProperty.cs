namespace TreeStore.Core.Capabilities;

public interface ISetItemProperty
{
    /// <summary>
    /// Returns custom parameters to be applied for the setting of item properties in
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? SetItemPropertyParameters(PSObject properties) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Set the given item properties
    /// </summary>
    public void SetItemProperty(ICmdletProvider provider, PSObject properties);
}