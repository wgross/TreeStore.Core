using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Capabilities;

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
    public ProviderNode? MoveChildItem(CmdletProvider provider, ContainerNode parentOfNodeToMove, ProviderNode nodeToMove, string[] destination);
}