using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace PowerShellFilesystemProviderBase.Providers
{
    public partial class PowerShellFileSystemProviderBase : ContainerCmdletProvider
    {
        private PowershellFileSystemDriveInfo DriveInfo => (PowershellFileSystemDriveInfo)this.PSDriveInfo;

        public (bool exists, ProviderNode? node) TryGetChildNode(ContainerNode parentNode, string childName)
        {
            var childNode = parentNode
                .GetChildItems()
                .FirstOrDefault(n => StringComparer.OrdinalIgnoreCase.Equals(n.Name, childName));

            return (childNode is not null, childNode);
        }

        protected bool TryGetNodeByPath(string path, [NotNullWhen(returnValue: true)] out ProviderNode? pathNode)
        {
            pathNode = default;
            (bool exists, ProviderNode? node) traversal = (true, this.DriveInfo.RootNode);
            foreach (var pathItem in new PathTool().Split(path))
            {
                traversal = traversal.node switch
                {
                    ContainerNode container => this.TryGetChildNode(container, pathItem),
                    _ => (false, default)
                };
                if (!traversal.exists) return false;
            }
            pathNode = traversal.node;
            return true;
        }

        protected bool TryGetNodeByPath<T>(string path, [NotNullWhen(returnValue: true)] out ProviderNode? providerNode, [NotNullWhen(returnValue: true)] out T? providerNodeCapability) where T : class
            => this.TryGetNodeByPath<T>(this.DriveInfo.RootNode, new PathTool().Split(path), out providerNode, out providerNodeCapability);

        protected bool TryGetNodeByPath<T>(ProviderNode startNode, string[] path, [NotNullWhen(returnValue: true)] out ProviderNode? providerNode, [NotNullWhen(returnValue: true)] out T? providerNodeCapbility) where T : class
        {
            providerNode = default;
            providerNodeCapbility = default;

            (bool exists, ProviderNode? node) cursor = (true, startNode);
            foreach (var pathItem in path)
            {
                cursor = cursor.node switch
                {
                    ContainerNode container => this.TryGetChildNode(container, pathItem),
                    _ => (false, default)
                };
                if (!cursor.exists) return false;
            }

            providerNode = cursor.node;
            providerNodeCapbility = cursor.node as T;
            return providerNodeCapbility is not null;
        }

        #region Write a ProviderNode to pipeline

        protected void WriteProviderNode(string path, ProviderNode node)
        {
            var psobject = node.GetItem();
            if (psobject is null)
                return;

            this.WriteItemObject(
                item: this.DecorateItem(path, psobject),
                path: this.DecoratePath(path),
                isContainer: node.IsContainer);
        }

        protected PSObject DecorateItem(string path, PSObject psobject)
        {
            psobject.Properties.Add(new PSNoteProperty("PSParentPath",
                @$"{this.PSDriveInfo.Provider.ModuleName}\{this.PSDriveInfo.Provider.Name}::{this.PSDriveInfo.Name}:\{Path.GetDirectoryName(path)}"));
            return psobject;
        }

        protected string DecoratePath(string path) => @$"{this.PSDriveInfo.Provider.ModuleName}\{this.PSDriveInfo.Provider.Name}::{this.PSDriveInfo.Name}:\{path}";

        #endregion Write a ProviderNode to pipeline
    }
}