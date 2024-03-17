using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers;

public abstract partial class TreeStoreCmdletProviderBase : NavigationCmdletProvider
{
    #region Maintain a reference to the drive state

    private TreeStoreDriveInfoBase? treeStoreDriveInfo = null;

    private TreeStoreDriveInfoBase TreeStoreDriveInfo
        => this.treeStoreDriveInfo ?? (TreeStoreDriveInfoBase)this.PSDriveInfo;

    private TreeStoreDriveInfoBase GetTreeStoreDriveInfo(string? driveName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(driveName, nameof(driveName));

        if (this.SessionState.Drive.Get(driveName) is TreeStoreDriveInfoBase { } treeStoreDriveInfo)
            return treeStoreDriveInfo;

        throw new InvalidOperationException(string.Format(Resources.Error_UnkownTreeStoreDriveName, driveName));
    }

    /// <summary>
    /// Fetches the <see cref="TreeStoreDriveInfoBase"/> derived drive info by name from
    /// PowerShells <see cref="DriveManagementIntrinsics"/> by name <paramref name="driveName"/>.
    /// </summary>
    protected T GetTreeStoreDriveInfo<T>(string? driveName)
        where T : TreeStoreDriveInfoBase
    {
        if (this.GetTreeStoreDriveInfo(driveName) is T treeStoreDriveInfo)
            return treeStoreDriveInfo!;

        throw new InvalidOperationException(string.Format(Resources.Error_UnkownTreeStoreDriveName, driveName));
    }

    #endregion Maintain a reference to the drive state

    #region Traverse paths in drive

    private RootNode RootNode<T>(T driveInfo) where T : TreeStoreDriveInfoBase
        => new RootNode(this, driveInfo.GetRootNodeProvider());

    /// <summary>
    /// At the given tree store drive <paramref name="driveInfo"/> traverse from the root node along the path <paramref name="path"/>
    /// until the path ends. the last path items node id returned as <paramref name="pathNode"/>. If traversal fails before false is returned.
    /// </summary>
    protected bool TryGetNodeByPath<T>(T driveInfo, string[] path, [NotNullWhen(true)] out ProviderNode? pathNode) where T : TreeStoreDriveInfoBase
    {
        pathNode = default;

        // start the traversal at the root node.
        (bool exists, ProviderNode? node) cursor = (true, this.RootNode(driveInfo));

        foreach (var pathItem in path)
        {
            cursor = cursor.node switch
            {
                // for a container node fetch the child by name
                ContainerNode container => container.TryGetChildNode(pathItem, out var childNode) ? (true, childNode) : (false, default),

                // in any other case fails
                _ => (false, default)
            };

            if (!cursor.exists)
                return false;
        }
        pathNode = cursor.node!;

        return true;
    }

    /// <summary>
    /// From the current root node a given path <paramref name="path"/> is traversed as depp as possible.
    /// The deepest node is returned and the part of the path tht couldn't be retrieved is retuned in <paramref name="missingPath"/>.
    /// </summary>
    protected ProviderNode GetDeepestNodeByPath<T>(T driveInfo, string[] path, out string[] missingPath) where T : TreeStoreDriveInfoBase
    {
        var startNode = this.RootNode(driveInfo);
        var traversal = this.TraversePathComplete(startNode, path).ToArray();

        // the whole path might not exists
        missingPath = traversal.SkipWhile(t => t.exists).Select(t => t.name).ToArray();

        // at least the start node exists
        return traversal.TakeWhile(t => t.node is not null).LastOrDefault().node ?? startNode;
    }

    private IEnumerable<(string name, bool exists, ProviderNode? node)> TraversePathComplete(ProviderNode startNode, string[] path)
    {
        (bool exists, ProviderNode? node) cursor = (true, startNode);

        foreach (var pathItem in path)
        {
            if (cursor.exists)
            {
                // try to descend
                cursor = cursor.node switch
                {
                    // you can only fetch child node from containers
                    ContainerNode c => c.TryGetChildNode(pathItem, out var childNode) ? (true, childNode) : (false, null),

                    // from leaf nodes, no child can be retrieved
                    _ => (false, null)
                };

                // the result of the last traversal is returned
                yield return (
                    name: pathItem, cursor.exists, cursor.node
                );
            }
            else
            {
                // path points where new node exists
                yield return (
                    name: pathItem,
                    exists: false,
                    node: null
                );
            }
        }
    }

    #endregion Traverse paths in drive

    #region Write a ProviderNode to pipeline

    protected void WriteProviderNode(string path, ProviderNode node)
    {
        // PowerShell would wrap the underlying with a PSObject itself
        // but taking the PSObject from the node allows to provide additional properties
        var psobject = node.GetItem();
        if (psobject is null)
            return;

        this.WriteItemObject(
            item: psobject,
            path: path,
            isContainer: node is ContainerNode);
    }

    #endregion Write a ProviderNode to pipeline

    #region Invoke a node capability

    /// <summary>
    /// Invokes a member of <see cref="ContainerNode"/>. If the node isn't a container the <paramref name="fallback"/> is invoked.
    /// </summary>
    protected T InvokeContainerNodeOrDefault<T, D>(D driveInfo, string[] path, Func<ContainerNode, T> invoke, Func<T> fallback)
        where D : TreeStoreDriveInfoBase
        => this.TryGetContainerNodeByPath(driveInfo, path, out var containerNode) ? invoke(containerNode) : fallback();

    /// <summary>
    /// Invokes a member of <see cref="ContainerNode"/>. If the node isn't a container the <paramref name="fallback"/> is invoked.
    /// If there is no node at all an <see cref="ItemNotFoundException"/> is thrown.
    /// </summary>
    protected void InvokeContainerNodeOrDefault<T>(T driveInfo, string[] path, Action<ContainerNode> invoke, Action fallback)
        where T : TreeStoreDriveInfoBase
    {
        if (this.TryGetContainerNodeByPath(driveInfo, path, out var containerNode))
        {
            invoke(containerNode);
        }
        else
        {
            fallback();
        }
    }

    /// <summary>
    /// At the node at <paramref name="path"/> invoke the function <paramref name="invoke"/>. If the node isn't found the <paramref name="fallback"/> is invoked.
    /// Path is traverse from root node of <paramref name="driveInfo"/>.
    /// If there is no node at all an <see cref="ItemNotFoundException"/> is thrown.
    /// </summary>
    protected T? InvokeProviderNodeOrDefault<T, D>(D driveInfo, string[] path, Func<ProviderNode, T> invoke, Func<T> fallback)
        where D : TreeStoreDriveInfoBase
    {
        return this.TryGetNodeByPath(driveInfo, path, out var node) ? invoke(node) : fallback();
    }

    /// <summary>
    /// Retrieve the node at <paramref name="path"/>. Path traversal begins at <paramref name="containerNode"/> at drive <paramref name="driveInfo"/>.
    /// If the node isn't found an <see cref="ItemNotFoundException"/> is thrown.
    /// </summary>
    protected bool TryGetContainerNodeByPath<T>(T driveInfo, string[] path, [NotNullWhen(true)] out ContainerNode? containerNode)
        where T : TreeStoreDriveInfoBase
    {
        if (this.TryGetNodeByPath(driveInfo, path, out var providerNode))
        {
            if (providerNode is ContainerNode container)
            {
                containerNode = container;
                return true;
            }

            containerNode = default;
            return false;
        }
        else
        {
            throw new ItemNotFoundException(string.Format(Resources.Error_CantFindPath, string.Join("\\", path)));
        }
    }

    #endregion Invoke a node capability
}