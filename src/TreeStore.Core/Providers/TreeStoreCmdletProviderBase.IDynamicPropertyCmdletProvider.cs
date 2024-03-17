namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase : IDynamicPropertyCmdletProvider
{
    /// <inheritdoc/>
    public void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
    {
        var sourceSplitted = PathTool.Default.SplitProviderQualifiedPath(sourcePath);
        var sourceDriveInfo = this.GetTreeStoreDriveInfo(sourceSplitted.DriveName);

        var destinationSplitted = PathTool.Default.SplitProviderQualifiedPath(destinationPath);
        var destinationDriveInfo = this.GetTreeStoreDriveInfo(destinationSplitted.DriveName);

        if (this.TryGetNodeByPath(sourceDriveInfo, sourceSplitted.Items, out var sourceNode) && this.TryGetNodeByPath(destinationDriveInfo, destinationSplitted.Items, out var destinationNode))
        {
            destinationNode.CopyItemProperty(sourceNode, sourceProperty, destinationProperty);
        }
    }

    /// <inheritdoc/>
    public object? CopyPropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
    {
        var sourceSplitted = PathTool.Default.SplitProviderQualifiedPath(sourcePath);
        var sourceDriveInfo = this.GetTreeStoreDriveInfo(sourceSplitted.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: sourceDriveInfo,
            path: sourceSplitted.Items,
            invoke: n => n.CopyItemPropertyParameters(sourcePath, sourceProperty, destinationPath, destinationProperty),
            fallback: () => null);
    }

    /// <inheritdoc/>
    public void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
    {
        var sourceSplitted = PathTool.Default.SplitProviderQualifiedPath(sourcePath);
        var sourceDriveInfo = this.GetTreeStoreDriveInfo(sourceSplitted.DriveName);

        var destinationSplitted = PathTool.Default.SplitProviderQualifiedPath(destinationPath);
        var destinationDriveInfo = this.GetTreeStoreDriveInfo(destinationSplitted.DriveName);

        if (this.TryGetNodeByPath(sourceDriveInfo, sourceSplitted.Items, out var sourceNode) && this.TryGetNodeByPath(destinationDriveInfo, destinationSplitted.Items, out var destinationNode))
        {
            destinationNode.MoveItemProperty(sourceNode, sourceProperty, destinationProperty);
        }
    }

    /// <inheritdoc/>
    public object? MovePropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
    {
        var sourceSplitted = PathTool.Default.SplitProviderQualifiedPath(sourcePath);
        var sourceDriveInfo = this.GetTreeStoreDriveInfo(sourceSplitted.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: sourceDriveInfo,
            path: sourceSplitted.Items,
            invoke: n => n.MoveItemPropertyParameters(sourcePath, sourceProperty, destinationPath, destinationProperty),
            fallback: () => null);
    }

    /// <inheritdoc/>
    public void NewProperty(string path, string propertyName, string propertyTypeName, object? value)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var providerNode))
        {
            providerNode.NewItemProperty(propertyName, propertyTypeName, value);
        }
    }

    /// <inheritdoc/>
    public object? NewPropertyDynamicParameters(string path, string propertyName, string propertyTypeName, object? value)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
            invoke: n => n.NewItemPropertyParameter(propertyName, propertyTypeName, value),
            fallback: () => null);
    }

    /// <inheritdoc/>
    public void RemoveProperty(string path, string propertyName)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var providerNode))
        {
            providerNode.RemoveItemProperty(propertyName);
        }
    }

    /// <inheritdoc/>
    public object RemovePropertyDynamicParameters(string path, string propertyName)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
            invoke: n => n.RemoveItemPropertyParameters(propertyName),
            fallback: () => default) ?? default!; // satisfy interface definition
    }

    /// <inheritdoc/>
    public void RenameProperty(string path, string sourceProperty, string destinationProperty)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var providerNode))
        {
            providerNode.RenameItemProperty(sourceProperty, destinationProperty);
        }
    }

    /// <inheritdoc/>
    public object? RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
            invoke: n => n.RenameItemPropertyParameters(sourceProperty, destinationProperty),
            fallback: () => null);
    }
}