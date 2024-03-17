namespace TreeStore.Core.Capabilities;

public interface ISetChildItemContent
{
    /// <summary>
    /// Returns custom parameters to be applied for the getting an items content
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? SetChildItemContentParameters(string childName) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Retrieves the content from an item.
    /// </summary>
    public IContentWriter? GetChildItemContentWriter(ICmdletProvider provider, string childName);
}