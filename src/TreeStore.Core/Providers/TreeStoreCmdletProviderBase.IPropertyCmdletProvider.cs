using System.Collections.ObjectModel;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase : IPropertyCmdletProvider
{
    /// <inheritdoc/>
    public void ClearProperty(string path, Collection<string> propertyToClear)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var providerNode))
        {
            providerNode.ClearItemProperty(propertyToClear);
        }
    }

    /// <inheritdoc/>
    public object? ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitPath.Items,
            invoke: n => n.ClearItemPropertyParameters(propertyToClear),
            fallback: () => null);
    }

    /// <inheritdoc/>
    public void GetProperty(string path, Collection<string>? providerSpecificPickList)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var providerNode))
        {
            var pso = providerNode.GetItemProperty(providerSpecificPickList);

            this.WriteItemObject(
                item: pso,
                path: path,
                isContainer: providerNode is ContainerNode);
        }
    }

    /// <inheritdoc/>
    public object? GetPropertyDynamicParameters(string path, Collection<string>? providerSpecificPickList)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitPath.Items,
            invoke: n => n.GetItemPropertyParameters(providerSpecificPickList),
            fallback: () => null);
    }

    /// <inheritdoc/>
    public void SetProperty(string path, PSObject propertyValue)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitPath.Items, out var providerNode))
        {
            providerNode.SetItemProperty(propertyValue);
        }
    }

    /// <inheritdoc/>
    public object? SetPropertyDynamicParameters(string path, PSObject propertyValue)
    {
        var splitPath = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitPath.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitPath.Items,
            invoke: n => n.SetItemPropertyParameters(propertyValue),
            fallback: () => null);
    }
}