using System.Management.Automation.Provider;

namespace PowerShellFilesystemProviderBase.Providers
{
    public partial class PowerShellFileSystemProviderBase : IDynamicPropertyCmdletProvider
    {
        public void CopyProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            if (this.TryGetNodeByPath(sourcePath, out var sourceNode) && this.TryGetNodeByPath(destinationPath, out var destinationNode))
            {
                destinationNode.CopyItemProperty(sourceNode, sourceProperty, destinationProperty);
            }
        }

        public object? CopyPropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
            => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(destinationPath).path.items,
                invoke: n => n.CopyItemPropertyParameters(sourcePath, sourceProperty, destinationPath, destinationProperty),
                fallback: () => null);

        public void MoveProperty(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        {
            if (this.TryGetNodeByPath(sourcePath, out var sourceNode) && this.TryGetNodeByPath(destinationPath, out var destinationNode))
            {
                destinationNode.MoveItemProperty(sourceNode, sourceProperty, destinationProperty);
            }
        }

        public object? MovePropertyDynamicParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
            => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(destinationPath).path.items,
                invoke: n => n.MoveItemPropertyParameters(sourcePath, sourceProperty, destinationPath, destinationProperty),
                fallback: () => null);

        public void NewProperty(string path, string propertyName, string propertyTypeName, object value)
        {
            if (this.TryGetNodeByPath(path, out var providerNode))
            {
                providerNode.NewItemProperty(propertyName, propertyTypeName, value);
            }
        }

        public object? NewPropertyDynamicParameters(string path, string propertyName, string propertyTypeName, object value)
            => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n.NewItemPropertyParameter(propertyName, propertyTypeName, value),
                fallback: () => null);

        public void RemoveProperty(string path, string propertyName)
        {
            if (this.TryGetNodeByPath(path, out var providerNode))
            {
                providerNode.RemoveItemProperty(propertyName);
            }
        }

        public object? RemovePropertyDynamicParameters(string path, string propertyName)
            => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n.RemoveItemPropertyParameters(propertyName),
                fallback: () => null);

        public void RenameProperty(string path, string sourceProperty, string destinationProperty)
        {
            if (this.TryGetNodeByPath(path, out var providerNode))
            {
                providerNode.RenameItemProperty(sourceProperty, destinationProperty);
            }
        }

        public object? RenamePropertyDynamicParameters(string path, string sourceProperty, string destinationProperty)
            => this.InvokeProviderNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n.RenameItemPropertyParameters(sourceProperty, destinationProperty),
                fallback: () => null);
    }
}