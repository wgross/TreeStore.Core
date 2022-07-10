using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Capabilities;

/// <summary>
/// Communicate the copying of a node (successful or failed) to the invoking provider.
/// </summary>
public record class MoveChildItemResult(bool Created, string? Name, IServiceProvider? NodeServices);

public interface IMoveChildItem
{
    /// <summary>
    /// Returns custom parameters to be applied for moving a child node named <paramref name="childName"/> to a destination.
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? MoveChildItemParameters(string childName, string destination) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Move the node <paramref name="nodeToMove"/> as a child under 'this' node.
    /// </summary>
    public MoveChildItemResult MoveChildItem(CmdletProvider provider, ContainerNode parentOfNodeToMove, ProviderNode nodeToMove, string[] destination);
}