using System.IO;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase
{
    /// <inheritdoc/>
    protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
    {
        return base.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
    }

    /// <inheritdoc/>
    protected override void CopyItem(string path, string destination, bool recurse)
    {
        var splittedSource = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splittedSource.ParentAndChild;

        var sourceDriveInfo = this.GetTreeStoreDriveInfo(splittedSource.DriveName);

        this.InvokeContainerNodeOrDefault(
            driveInfo: sourceDriveInfo,
            path: parentPath,
            invoke: sourceParentNode =>
            {
                // first check that node to copy exists
                if (!sourceParentNode.TryGetChildNode(childName!, out var childNodeToCopy))
                    throw new InvalidOperationException(string.Format(Resources.Error_CanFindFileSystemItem, path));

                // check if the destination node is at the same provider
                var destinationPathSplitted = PathTool.Default.SplitProviderQualifiedPath(destination);

                var destinationDriveInfo = this.GetTreeStoreDriveInfo(destinationPathSplitted.DriveName);

                if (string.IsNullOrEmpty(destinationPathSplitted.DriveName) || sourceDriveInfo.Equals(destinationDriveInfo))
                {
                    // find the deepest ancestor which serves as a destination to copy to
                    var destinationAncestor = this.GetDeepestNodeByPath(destinationDriveInfo, destinationPathSplitted.Items, out var missingPath);

                    if (destinationAncestor is ContainerNode destinationAncestorContainer)
                    {
                        // destination ancestor is a container and might accept the copy
                        destinationAncestorContainer.CopyChildItem(childNodeToCopy, destination: missingPath, recurse);
                    }
                    else
                    {
                        base.CopyItem(path, destination, recurse);
                    }
                }
                else
                {
                    sourceParentNode.CopyChildItemToProvider(childNodeToCopy, destinationDriveInfo, destination, recurse);
                }
            },
            fallback: () => base.CopyItem(path, destination, recurse));
    }

    /// <inheritdoc/>
    protected override object? CopyItemDynamicParameters(string path, string destination, bool recurse)
    {
        var sourceSplitted = PathTool.Default.SplitProviderQualifiedPath(path);
        var sourceDriveInfo = this.GetTreeStoreDriveInfo(sourceSplitted.DriveName);

        return this.InvokeContainerNodeOrDefault(
            driveInfo: sourceDriveInfo,
            path: sourceSplitted.Items,
            invoke: c => c.CopyChildItemParameters(path, destination, recurse),
            fallback: () => base.CopyItemDynamicParameters(path, destination, recurse));
    }

    /// <summary>
    /// this isn't used. The base class method in <see cref="ContainerCmdletProvider"/> is called if depth is <see cref="uint.MaxValue"/>.
    /// </summary>
    //protected override void GetChildItems(string path, bool recurse)
    //{
    //    base.GetChildItems(path, recurse);
    //}

    /// <inheritdoc/>
    protected override void GetChildItems(string path, bool recurse, uint depth)
    {
        void writeItemObject(PSObject item, string path, string name, bool isContainer) => this.WriteItemObject(item, path, isContainer);

        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        this.InvokeContainerNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
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

    /// <inheritdoc/>
    protected override object? GetChildItemsDynamicParameters(string path, bool recurse)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeContainerNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
            invoke: c => c.GetChildItemParameters(path, recurse),
            fallback: () => base.GetChildItemsDynamicParameters(path, recurse));
    }

    /// <inheritdoc/>
    protected override void GetChildNames(string path, ReturnContainers returnContainers)
    {
        void writeItemObject(PSObject _, string path, string name, bool isContainer) => this.WriteItemObject(name, path, isContainer);

        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        this.InvokeContainerNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
            invoke: c => this.GetChildItems(parentContainer: c, path, recurse: false, depth: 0, writeItemObject),
            fallback: () => base.GetChildNames(path, returnContainers));
    }

    /// <inheritdoc/>
    protected override object? GetChildNamesDynamicParameters(string path) => this.GetChildItemsDynamicParameters(path, recurse: false);

    /// <inheritdoc/>
    protected override bool HasChildItems(string path)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
            invoke: n => n switch
            {
                ContainerNode c => c.HasChildItems(provider: this),

                // LeafNodes never have children
                _ => false
            },
            fallback: () => base.HasChildItems(path));
    }

    /// <inheritdoc/>
    protected override void RemoveItem(string path, bool recurse)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitted.ParentAndChild;

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        this.InvokeContainerNodeOrDefault(
            driveInfo: driveInfo,
            path: parentPath,
            invoke: c => c.RemoveChildItem(childName!, recurse),
            fallback: () => base.RemoveItem(path, recurse));
    }

    /// <inheritdoc/>
    protected override object? RemoveItemDynamicParameters(string path, bool recurse)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitted.ParentAndChild;

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeContainerNodeOrDefault(
            driveInfo: driveInfo,
            path: parentPath,
            invoke: c => c.RemoveChildItemParameters(childName!, recurse),
            fallback: () => base.RemoveItemDynamicParameters(path, recurse));
    }

    /// <inheritdoc/>
    protected override void NewItem(string path, string itemTypeName, object newItemValue)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        var (parentPath, childName) = splitted.ParentAndChild;

        if (this.TryGetNodeByPath(driveInfo, parentPath, out var parentNode))
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

    /// <inheritdoc/>
    protected override object? NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitted.ParentAndChild;

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeContainerNodeOrDefault(
            driveInfo: driveInfo,
            path: parentPath,
            invoke: c => c.NewChildItemParameters(childName!, itemTypeName, newItemValue),
            fallback: () => base.NewItemDynamicParameters(path, itemTypeName, newItemValue));
    }

    /// <inheritdoc/>
    protected override void RenameItem(string path, string newName)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitted.ParentAndChild;

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, parentPath, out var providerNode))
        {
            if (providerNode is ContainerNode parentContainer)
            {
                parentContainer.RenameChildItem(childName!, newName);
            }
        }
    }

    /// <inheritdoc/>
    protected override object? RenameItemDynamicParameters(string path, string newName)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitted.ParentAndChild;

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeContainerNodeOrDefault(
            driveInfo: driveInfo,
            path: parentPath,
            invoke: c => c.RenameChildItemParameters(childName!, newName),
            fallback: () => base.RenameItemDynamicParameters(path, newName));
    }
}