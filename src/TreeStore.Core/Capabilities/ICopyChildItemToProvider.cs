using TreeStore.Core.Nodes;

namespace TreeStore.Core.Capabilities;

/// <summary>
/// Communicate the copying of a node (successful or failed) to the invoking provider.
/// </summary>
//public record class CopyChildItemResult(bool Created, string? Name, IServiceProvider? NodeServices);

public interface ICopyChildItemToProvider
{
    /// <summary>
    /// Returns custom parameters to be applied for the copying a child node named <paramref name="childName"/> to a destination.
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    // public object? CopyChildItemParameters(string childName, string destination, bool recurse) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Creates a new child node at <paramref name="destination"/> from the given <paramref name="nodeToCopy"/>
    /// </summary>
    public CopyChildItemResult CopyChildItem(ICmdletProvider provider, ProviderNode nodeToCopy, ProviderInfo destinationProvider, PSDriveInfo destinationDrive, string destination, bool recurse);
}