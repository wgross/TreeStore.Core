using System.Collections.ObjectModel;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase : IPropertyCmdletProvider
{
    /// <inheritdoc/>
    public void ClearProperty(string path, Collection<string> propertyToClear)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var providerNode))
        {
            providerNode.ClearItemProperty(propertyToClear);
        }
    }

    /// <inheritdoc/>
    public object? ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
            invoke: n => n.ClearItemPropertyParameters(propertyToClear),
            fallback: () => null);
    }

    /// <inheritdoc/>
    public void GetProperty(string path, Collection<string>? providerSpecificPickList)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var providerNode))
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
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
            invoke: n => n.GetItemPropertyParameters(providerSpecificPickList),
            fallback: () => null);
    }

    /// <inheritdoc/>
    public void SetProperty(string path, PSObject propertyValue)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        if (this.TryGetNodeByPath(driveInfo, splitted.Items, out var providerNode))
        {
            providerNode.SetItemProperty(propertyValue);
        }
    }

    /// <inheritdoc/>
    public object? SetPropertyDynamicParameters(string path, PSObject propertyValue)
    {
        var splitted = PathTool.Default.SplitProviderQualifiedPath(path);

        var driveInfo = this.GetTreeStoreDriveInfo(splitted.DriveName);

        return this.InvokeProviderNodeOrDefault(
            driveInfo: driveInfo,
            path: splitted.Items,
            invoke: n => n.SetItemPropertyParameters(propertyValue),
            fallback: () => null);
    }
}