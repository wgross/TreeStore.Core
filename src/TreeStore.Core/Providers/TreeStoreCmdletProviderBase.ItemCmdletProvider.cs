namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase
{
    /// <inheritdoc/>
    protected override void ClearItem(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            node.ClearItem();
        }
    }

    /// <inheritdoc/>
    protected override object? ClearItemDynamicParameters(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            return node.ClearItemParameters();
        }
        else return null;
    }

    /// <inheritdoc/>
    protected override void SetItem(string path, object value)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            node.SetItem(value);
        }
    }

    /// <inheritdoc/>
    protected override object? SetItemDynamicParameters(string path, object value)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            return node.SetItemParameters();
        }
        else return null;
    }

    // override not necessary.
    //protected override string[] ExpandPath(string path)
    //{
    //    return base.ExpandPath(path);
    //}

    /// <inheritdoc/>
    protected override void InvokeDefaultAction(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            node.InvokeItem();
        }
    }

    /// <inheritdoc/>
    protected override object? InvokeDefaultActionDynamicParameters(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            return node.InvokeItemParameters();
        }
        else return null;
    }

    /// <inheritdoc/>
    protected override void GetItem(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            this.WriteProviderNode(path, node);
        }
    }

    /// <inheritdoc/>
    protected override object? GetItemDynamicParameters(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            return node.GetItemParameters();
        }
        else return null;
    }

    /// <inheritdoc/>
    protected override bool IsValidPath(string path)
    {
        throw new System.NotImplementedException();
    }

    /// <inheritdoc/>
    protected override bool ItemExists(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            return node.ItemExists();
        }
        else return false;
    }

    /// <inheritdoc/>
    protected override object? ItemExistsDynamicParameters(string path)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var node))
        {
            return node.ItemExistsParameters();
        }
        else return null;
    }
}