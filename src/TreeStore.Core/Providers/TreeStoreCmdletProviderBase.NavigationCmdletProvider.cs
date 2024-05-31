using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase
{
    /// <inheritdoc/>
    protected override string GetParentPath(string path, string root) => base.GetParentPath(path, root);

    /// <inheritdoc/>
    protected override string GetChildName(string path) => base.GetChildName(path);

    /// <inheritdoc/>
    protected override bool IsItemContainer(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        return this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node)
            ? node is ContainerNode
            : false;
    }

    /// <inheritdoc/>
    protected override void MoveItem(string path, string destination)
    {
        var splitSourcePath = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitSourcePath.ParentAndChild;

        var sourceDriveInfo = this.GetTreeStoreDriveInfo(splitSourcePath.DriveName);

        this.InvokeContainerNodeOrDefault(
            driveInfo: sourceDriveInfo,
            path: parentPath,
            invoke: sourceParentNode =>
            {
                if (!sourceParentNode.TryGetChildNode(childName!, out var childNodeToMove))
                    throw new InvalidOperationException(string.Format(Resources.Error_CanFindFileSystemItem, path));

                var splitDestinationPath = PathTool.Default.SplitProviderQualifiedPath(destination);

                // find the deepest ancestor which serves as a destination to copy to
                // only moving with this provider is supported.
                var destinationAncestor = this.GetDeepestNodeByPath(sourceDriveInfo, splitDestinationPath.Items, out var missingPath);

                if (destinationAncestor is ContainerNode destinationAncestorContainer)
                {
                    // destination ancestor is a container and might accept the move operation
                    destinationAncestorContainer.MoveChildItem(sourceParentNode, childNodeToMove, destination: missingPath);
                }
                else
                {
                    // delegate to the base provider
                    base.MoveItem(path, destination);
                }
            },
            fallback: () => base.MoveItem(path, destination));
    }

    /// <inheritdoc/>
    protected override object? MoveItemDynamicParameters(string path, string destination)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        return this.InvokeContainerNodeOrDefault(
            driveInfo: driveInfo,
            path: splitPath.Items,
            invoke: c => c.MoveChildItemParameters(path, destination),
            fallback: () => base.MoveItemDynamicParameters(path, destination));
    }

    /// <inheritdoc/>
    protected override string MakePath(string parent, string child) => base.MakePath(parent, child);

    /// <inheritdoc/>
    protected override string NormalizeRelativePath(string path, string basePath) => base.NormalizeRelativePath(path, basePath);
}