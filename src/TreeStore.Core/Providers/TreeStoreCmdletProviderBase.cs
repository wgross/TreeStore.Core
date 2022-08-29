﻿using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers;

public abstract partial class TreeStoreCmdletProviderBase : NavigationCmdletProvider
{
    private TreeStoreDriveInfoBase? treeStoreDriveInfo = null;

    private TreeStoreDriveInfoBase? TreeStoreDriveInfo
        => this.treeStoreDriveInfo ?? (TreeStoreDriveInfoBase)this.PSDriveInfo;

    /// <summary>
    /// Create a <see cref="RootNode"/> instance from the <see cref="IServiceProvider"/> representing the root nodes
    /// payload
    /// </summary>
    private RootNode RootNode() => new RootNode(this, this.TreeStoreDriveInfo.GetRootNodeProvider());

    public (bool exists, ProviderNode? node) TryGetChildNode(ContainerNode parentNode, string childName)
    {
        var childNode = parentNode
            .GetChildItems(provider: this)
            .FirstOrDefault(n => StringComparer.OrdinalIgnoreCase.Equals(n.Name, childName));

        return (childNode is not null, childNode);
    }

    private void EnsureTreeStoreDriveInfo(string? drive)
    {
        if (drive is not null && this.PSDriveInfo is null)
        {
            this.treeStoreDriveInfo = SessionState.Drive.Get(drive) as TreeStoreDriveInfoBase;
        };

        ArgumentNullException.ThrowIfNull(this.TreeStoreDriveInfo, nameof(PSDriveInfo));
    }

    protected bool TryGetNodeByPath(string path, [NotNullWhen(returnValue: true)] out ProviderNode? pathNode)
    {
        var splitted = new PathTool().SplitProviderPath(path);

        this.EnsureTreeStoreDriveInfo(splitted.drive);

        return this.TryGetNodeByPath(splitted.path.items, out pathNode);
    }

    protected bool TryGetNodeByPath(string[] path, [NotNullWhen(returnValue: true)] out ProviderNode? pathNode)
    {
        pathNode = default;
        (bool exists, ProviderNode? node) traversal = (true, this.RootNode());
        foreach (var pathItem in path)
        {
            traversal = traversal.node switch
            {
                ContainerNode container => TryGetChildNode(container, pathItem),
                _ => (false, default)
            };
            if (!traversal.exists) return false;
        }
        pathNode = traversal.node;
        return true;
    }

    protected ProviderNode GetDeepestNodeByPath(string path, out string[] missingPath)
        => GetDeepestNodeByPath(this.RootNode(), new PathTool().SplitProviderPath(path).path.items, out missingPath);

    protected ProviderNode GetDeepestNodeByPath(ProviderNode startNode, string[] path, out string[] missingPath)
    {
        var traversal = this.TraversePathComplete(startNode, path).ToArray();

        // the whole path might not exists
        missingPath = traversal.SkipWhile(t => t.exists).Select(t => t.name).ToArray();

        // at least the start node exists
        return traversal.TakeWhile(t => t.node is not null).LastOrDefault().node ?? startNode;
    }

    protected IEnumerable<(string name, bool exists, ProviderNode? node)> TraversePathComplete(ProviderNode startNode, string[] path)
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
                    ContainerNode c => this.TryGetChildNode(c, pathItem),

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
    /// Invokes a member of <see cref="ContainerNode"/>. If the node isn't a container the fallback is invoked
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <param name="invoke"></param>
    /// <param name="fallback"></param>
    /// <returns></returns>
    protected T InvokeContainerNodeOrDefault<T>(string[] path, Func<ContainerNode, T> invoke, Func<T> fallback)
    {
        return this.TryGetContainerNodeByPath(path, out var containerNode) ? invoke(containerNode) : fallback();
    }

    protected void InvokeContainerNodeOrDefault(string[] path, Action<ContainerNode> invoke, Action fallback)
    {
        if (this.TryGetContainerNodeByPath(path, out var containerNode))
        {
            invoke(containerNode);
        }
        else
        {
            fallback();
        }
    }

    protected bool TryGetContainerNodeByPath(string[] path, [NotNullWhen(true)] out ContainerNode? containerNode)
    {
        if (this.TryGetNodeByPath(path, out var providerNode))
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
            throw new ItemNotFoundException($"Can't find path '{string.Join("\\", path)}'");
        }
    }

    protected void InvokeProviderNodeOrDefault(string[] path, Action<ProviderNode> invoke, Action fallback)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            invoke(node);
        }
        else
        {
            fallback();
        }
    }

    protected T? InvokeProviderNodeOrDefault<T>(string[] path, Func<ProviderNode, T> invoke, Func<T> fallback)
    {
        return this.TryGetNodeByPath(path, out var node) ? invoke(node) : fallback();
    }

    #endregion Invoke a node capability
}