namespace TreeStore.Core.Capabilities;

/// <summary>
/// Communicate the creation of a node (successful or failed) to the invoking provider.
/// </summary>
public record class NewChildItemResult(bool Created, string? Name, IServiceProvider? NodeServices);

public interface INewChildItem
{
    /// <summary>
    /// Returns custom parameters to be applied for the creation a new child node named <paramref name="childName"/>
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? NewChildItemParameters(string childName, string itemTypeName, object newItemValue) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Creates a new child named <paramref name="childName"/>
    /// </summary>
    public NewChildItemResult NewChildItem(CmdletProvider provider, string childName, string? itemTypeName, object? newItemValue);
}