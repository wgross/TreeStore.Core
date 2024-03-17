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
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.TryGetNodeByPath(driveInfo, splitted.Items, out var node)
            ? node is ContainerNode
            : false;
    }

    /// <inheritdoc/>
    protected override void MoveItem(string path, string destination)
    {
        var splittedSourcePath = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splittedSourcePath.ParentAndChild;

        var sourceDriveInfo = this.GetTreeStoreDriveInfo(splittedSourcePath.DriveName);

        this.InvokeContainerNodeOrDefault(
            driveInfo: sourceDriveInfo,
            path: parentPath,
            invoke: sourceParentNode =>
            {
                if (!sourceParentNode.TryGetChildNode(childName!, out var childNodeToMove))
                    throw new InvalidOperationException(string.Format(Resources.Error_CanFindFileSystemItem, path));

                var splittedDestinationPath = PathTool.Default.SplitProviderQualifiedPath(destination);

                // find the deepest ancestor which serves as a destination to copy to
                // only moving with this provider is supported.
                var destinationAncestor = this.GetDeepestNodeByPath(sourceDriveInfo, splittedDestinationPath.Items, out var missingPath);

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
        var splittedPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splittedPath.DriveName);

        return this.InvokeContainerNodeOrDefault(
            driveInfo: driveInfo,
            path: splittedPath.Items,
            invoke: c => c.MoveChildItemParameters(path, destination),
            fallback: () => base.MoveItemDynamicParameters(path, destination));
    }

    /// <inheritdoc/>
    protected override string MakePath(string parent, string child) => base.MakePath(parent, child);

    /// <inheritdoc/>
    protected override string NormalizeRelativePath(string path, string basePath) => base.NormalizeRelativePath(path, basePath);
}