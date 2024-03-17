using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase : IContentCmdletProvider
{
    /// <inheritdoc/>
    public void ClearContent(string path)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var node))
        {
            node.ClearItemContent();
        }
    }

    /// <inheritdoc/>
    public object? ClearContentDynamicParameters(string path)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var node))
        {
            return node.ClearItemContentParameters();
        }
        else return null;
    }

    /// <inheritdoc/>
    public IContentReader? GetContentReader(string path)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var node))
        {
            return node.GetItemContentReader();
        }
        else return null;
    }

    /// <inheritdoc/>
    public object? GetContentReaderDynamicParameters(string path)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var node))
        {
            return node.GetItemContentParameters();
        }
        else return null;
    }

    /// <inheritdoc/>
    public IContentWriter? GetContentWriter(string path)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitted.ParentAndChild;

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

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
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var (parentPath, childName) = splitted.ParentAndChild;

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

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