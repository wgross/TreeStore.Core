using PowerShellFilesystemProviderBase.Nodes;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace PowerShellFilesystemProviderBase.Providers
{
    public partial class PowerShellFileSystemProviderBase : IPropertyCmdletProvider
    {
        public void ClearProperty(string path, Collection<string> propertyToClear)
        {
            if (this.TryGetNodeByPath(path, out var providerNode))
            {
                providerNode.ClearItemProperty(propertyToClear);
            }
        }

        public object? ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
             => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n.ClearItemPropertyParameters(propertyToClear),
                fallback: () => null);

        public void GetProperty(string path, Collection<string> providerSpecificPickList)
        {
            if (this.TryGetNodeByPath(path, out var providerNode))
            {
                var pso = providerNode.GetItemProperty(providerSpecificPickList);
                this.WriteItemObject(pso, path, isContainer: providerNode is ContainerNode);
            }
        }

        public object? GetPropertyDynamicParameters(string path, Collection<string> providerSpecificPickList)
             => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n.GetItemPropertyParameters(providerSpecificPickList),
                fallback: () => null);

        public void SetProperty(string path, PSObject propertyValue)
        {
            if (this.TryGetNodeByPath(path, out var providerNode))
            {
                providerNode.SetItemProperty(propertyValue);
            }
        }

        public object? SetPropertyDynamicParameters(string path, PSObject propertyValue)
            => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n.SetItemPropertyParameters(propertyValue),
                fallback: () => null);
    }
}