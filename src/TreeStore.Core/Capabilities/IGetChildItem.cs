using TreeStore.Core.Nodes;

namespace TreeStore.Core.Capabilities;

/// <summary>
/// Access a <sse cref="ProviderNode"/> child nodes.
/// </summary>
public interface IGetChildItem
{
    /// <summary>
    /// Dynamic parameters provided to PowerShells command 'Get-ChildItem'
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? GetChildItemParameters(string path, bool recurse) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Determines if this node has child nodes.
    /// </summary>
    public bool HasChildItems(CmdletProvider provider);

    /// <summary>
    /// Enumerates all child <see cref="ProviderNode"/>
    /// </summary>
    public IEnumerable<ProviderNode> GetChildItems(CmdletProvider provider);
}