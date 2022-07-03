using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers
{
    public partial class TreeStoreCmdletProviderBase : IPropertyCmdletProvider
    {
        public void ClearProperty(string path, Collection<string> propertyToClear)
        {
            if (this.TryGetNodeByPath(path, out var providerNode))
            {
                providerNode.ClearItemProperty(provider: this, propertyToClear);
            }
        }

        public object? ClearPropertyDynamicParameters(string path, Collection<string> propertyToClear)
             => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n.ClearItemPropertyParameters(propertyToClear),
                fallback: () => null);

        public void GetProperty(string path, Collection<string>? providerSpecificPickList)
        {
            if (this.TryGetNodeByPath(path, out var providerNode))
            {
                var pso = providerNode.GetItemProperty(provider: this, providerSpecificPickList);

                this.WriteItemObject(
                    item: pso,
                    path: path,
                    isContainer: providerNode is ContainerNode);
            }
        }

        public object? GetPropertyDynamicParameters(string path, Collection<string>? providerSpecificPickList)
             => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n.GetItemPropertyParameters(providerSpecificPickList),
                fallback: () => null);

        public void SetProperty(string path, PSObject propertyValue)
        {
            if (this.TryGetNodeByPath(path, out var providerNode))
            {
                providerNode.SetItemProperty(provider: this, propertyValue);
            }
        }

        public object? SetPropertyDynamicParameters(string path, PSObject propertyValue)
            => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n.SetItemPropertyParameters(propertyValue),
                fallback: () => null);
    }
}