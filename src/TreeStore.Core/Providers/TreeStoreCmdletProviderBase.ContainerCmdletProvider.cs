﻿using System.IO;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase
{
    protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
    {
        return base.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
    }

    /// <summary>
    /// Implements the copying of a this.ICmdletProvider item.
    /// The existence of the child node <paramref name="path"/> has been verified in <see cref="ItemExists(string)"/>.
    /// th destination path <paramref name="destination"/> is unverified.
    /// </summary>
    protected override void CopyItem(string path, string destination, bool recurse)
    {
        var (parentPath, childName) = new PathTool().SplitProviderQualifiedPath(path).ParentAndChild;

        this.InvokeContainerNodeOrDefault(
            path: parentPath,
            invoke: sourceParentNode =>
            {
                // first check that node to copy exists
                if (!sourceParentNode.TryGetChildNode(childName!, out var nodeToCopy))
                    throw new InvalidOperationException($"Item '{path}' doesn't exist");

                // find the deepest ancestor which serves as a destination to copy to
                var destinationAncestor = this.GetDeepestNodeByPath(destination, out var missingPath);

                if (destinationAncestor is ContainerNode destinationAncestorContainer)
                {
                    // destination ancestor is a container and might accept the copy
                    destinationAncestorContainer.CopyChildItem(nodeToCopy, destination: missingPath, recurse);
                }
                else
                {
                    base.CopyItem(path, destination, recurse);
                }
            },
            fallback: () => base.CopyItem(path, destination, recurse));
    }

    protected override object? CopyItemDynamicParameters(string path, string destination, bool recurse) => this.InvokeContainerNodeOrDefault(
        path: new PathTool().SplitProviderQualifiedPath(path).Items,
        invoke: c => c.CopyChildItemParameters(path, destination, recurse),
        fallback: () => base.CopyItemDynamicParameters(path, destination, recurse));

    /// <summary>
    /// this isn't used. The base class method in <see cref="ContainerCmdletProvider"/> is called if depth is <see cref="uint.MaxValue"/>.
    /// </summary>
    //protected override void GetChildItems(string path, bool recurse)
    //{
    //    base.GetChildItems(path, recurse);
    //}

    protected override void GetChildItems(string path, bool recurse, uint depth)
    {
        void writeItemObject(PSObject item, string path, string name, bool isContainer) => this.WriteItemObject(item, path, isContainer);

        this.InvokeContainerNodeOrDefault(
          path: new PathTool().SplitProviderQualifiedPath(path).Items,
          invoke: c => this.GetChildItems(parentContainer: c, path, recurse, depth, writeItemObject),
          fallback: () => base.GetChildItems(path, recurse, depth));
    }

    private void GetChildItems(ContainerNode parentContainer, string path, bool recurse, uint depth, Action<PSObject, string, string, bool> writeItemObject)
    {
        foreach (var childGetItem in parentContainer.GetChildItems(provider: this))
        {
            var childItemPSObject = childGetItem.GetItem();
            if (childItemPSObject is not null)
            {
                var childItemPath = Path.Join(path, childGetItem.Name);
                writeItemObject(childItemPSObject, childItemPath, childGetItem.Name, childGetItem is ContainerNode);

                //TODO: recurse in cmdlet this.ICmdletProvider will be slow if the underlying model could optimize fetching of data.
                // alternatives:
                // - let first container pull in the whole operation -> change IGetChildItems to GetChildItems( bool recurse, uint depth)
                // - notify first container of incoming request so it can prepare the fetch: IPrepareGetChildItems: Prepare(bool recurse, uint depth) then resurce in this.ICmdletProvider
                //   General solution would be to introduce a call context to allow an impl. to inspect the original request.
                if (recurse && depth > 0 && childGetItem is ContainerNode childContainer)
                    this.GetChildItems(childContainer, childItemPath, recurse, depth - 1, writeItemObject);
            }
        }
    }

    protected override object? GetChildItemsDynamicParameters(string path, bool recurse) => this.InvokeContainerNodeOrDefault(
        path: new PathTool().SplitProviderQualifiedPath(path).Items,
        invoke: c => c.GetChildItemParameters(path, recurse),
        fallback: () => base.GetChildItemsDynamicParameters(path, recurse));

    protected override void GetChildNames(string path, ReturnContainers returnContainers)
    {
        void writeItemObject(PSObject _, string path, string name, bool isContainer) => this.WriteItemObject(name, path, isContainer);

        this.InvokeContainerNodeOrDefault(
            path: new PathTool().SplitProviderQualifiedPath(path).Items,
            invoke: c => this.GetChildItems(parentContainer: c, path, recurse: false, depth: 0, writeItemObject),
            fallback: () => base.GetChildNames(path, returnContainers));
    }

    protected override object? GetChildNamesDynamicParameters(string path) => this.GetChildItemsDynamicParameters(path, recurse: false);

    protected override bool HasChildItems(string path)
    {
        return this.InvokeProviderNodeOrDefault<bool>(
            path: new PathTool().SplitProviderQualifiedPath(path).Items,
            invoke: n => n switch
            {
                ContainerNode c => c.HasChildItems(provider: this),

                // LeafNodes never have children
                _ => false
            },
            fallback: () => base.HasChildItems(path));
    }

    protected override void RemoveItem(string path, bool recurse)
    {
        var (parentPath, childName) = new PathTool().SplitProviderQualifiedPath(path).ParentAndChild;

        this.InvokeContainerNodeOrDefault(
            path: parentPath,
            invoke: c => c.RemoveChildItem(childName!, recurse),
            fallback: () => base.RemoveItem(path, recurse));
    }

    protected override object? RemoveItemDynamicParameters(string path, bool recurse)
    {
        var (parentPath, childName) = new PathTool().SplitProviderQualifiedPath(path).ParentAndChild;

        return this.InvokeContainerNodeOrDefault(parentPath,
           invoke: c => c.RemoveChildItemParameters(childName!, recurse),
           fallback: () => base.RemoveItemDynamicParameters(path, recurse));
    }

    protected override void NewItem(string path, string itemTypeName, object newItemValue)
    {
        var (parentPath, childName) = new PathTool().SplitProviderQualifiedPath(path).ParentAndChild;

        if (this.TryGetNodeByPath(parentPath, out var parentNode))
        {
            if (parentNode is ContainerNode parentContainer)
            {
                var result = parentContainer.NewChildItem(childName!, itemTypeName, newItemValue);

                if (result is not null and ContainerNode container)
                {
                    this.WriteProviderNode(path, container);
                }
                else if (result is not null and LeafNode leaf)
                {
                    this.WriteProviderNode(path, leaf);
                }
            }
        }
    }

    protected override object? NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
    {
        var (parentPath, childName) = new PathTool().SplitProviderQualifiedPath(path).ParentAndChild;

        return this.InvokeContainerNodeOrDefault(parentPath,
            invoke: c => c.NewChildItemParameters(childName!, itemTypeName, newItemValue),
            fallback: () => base.NewItemDynamicParameters(path, itemTypeName, newItemValue));
    }

    protected override void RenameItem(string path, string newName)
    {
        var (parentPath, childName) = new PathTool().SplitProviderQualifiedPath(path).ParentAndChild;
        if (this.TryGetNodeByPath(parentPath, out var providerNode))
        {
            if (providerNode is ContainerNode parentContainer)
            {
                parentContainer.RenameChildItem(childName!, newName);
            }
        }
    }

    protected override object? RenameItemDynamicParameters(string path, string newName)
    {
        var (parentPath, childName) = new PathTool().SplitProviderQualifiedPath(path).ParentAndChild;

        return this.InvokeContainerNodeOrDefault(parentPath,
            invoke: c => c.RenameChildItemParameters(childName!, newName),
            fallback: () => base.RenameItemDynamicParameters(path, newName));
    }
}