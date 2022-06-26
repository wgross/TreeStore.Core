using TreeStore.Core.Nodes;

namespace TreeStore.Core.Capabilities;

/// <summary>
/// Extends an underlying of a <see cref="ContainerNode"/> to process a recursive copy of a given node.
/// </summary>
public interface ICopyChildItemRecursive : ICopyChildItem
{
    /// <summary>
    /// Creates a new child node at <paramref name="destination"/> from the given <paramref name="nodeToCopy"/>
    /// as a recursive copy
    /// </summary>
    public ProviderNode? CopyChildItemRecursive(ProviderNode nodeToCopy, string[] destination);
}