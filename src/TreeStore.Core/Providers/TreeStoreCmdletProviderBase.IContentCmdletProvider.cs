using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase : IContentCmdletProvider
{
    /// <inheritdoc/>
    public void ClearContent(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            node.ClearItemContent();
        }
    }

    /// <inheritdoc/>
    public object? ClearContentDynamicParameters(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            return node.ClearItemContentParameters();
        }
        else return null;
    }

    /// <inheritdoc/>
    public IContentReader? GetContentReader(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            return node.GetItemContentReader();
        }
        else return null;
    }

    /// <inheritdoc/>
    public object? GetContentReaderDynamicParameters(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            return node.GetItemContentParameters();
        }
        else return null;
    }

    /// <inheritdoc/>
    public IContentWriter? GetContentWriter(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitPath.ParentAndChild;

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, parentPath, out var parentNode))
        {
            if (parentNode is ContainerNode parentContainer)
            {
                return parentContainer.GetChildItemContentWriter(childName!);
            }
        }

        return null;
    }

    /// <inheritdoc/>
    public object? GetContentWriterDynamicParameters(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitPath.ParentAndChild;

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, parentPath, out var parentNode))
        {
            if (parentNode is ContainerNode parentContainer)
            {
                return parentContainer.SetChildItemContentParameters(childName!);
            }
        }
        return null;
    }
}