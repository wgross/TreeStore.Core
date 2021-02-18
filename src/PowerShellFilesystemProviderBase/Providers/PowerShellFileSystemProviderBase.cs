using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation.Provider;

namespace PowerShellFilesystemProviderBase.Providers
{
    public partial class PowerShellFileSystemProviderBase : NavigationCmdletProvider
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
            var providerPath = new PathTool().SplitProviderPath(path);

            pathNode = default;
            (bool exists, ProviderNode? node) traversal = (true, this.DriveInfo.RootNode);
            foreach (var pathItem in providerPath.path.items)
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

        protected ProviderNode GetDeepestNodeByPath(string path, out string[] missingPath)
           => this.GetDeepestNodeByPath(this.DriveInfo.RootNode, new PathTool().Split(path), out missingPath);

        protected ProviderNode GetDeepestNodeByPath(ProviderNode startNode, string[] path, out string[] missingPath)
        {
            var traversal = this.TraversePathComplete(startNode, path).ToArray();

            // the whole path might not exists
            missingPath = traversal.SkipWhile(t => t.exists).Select(t => t.name).ToArray();

            // at least the start node exists
            return traversal.TakeWhile(t => t.node is not null).LastOrDefault().node ?? startNode;
        }

        protected IEnumerable<(string name, bool exists, ProviderNode? node)> TraversePathComplete(ProviderNode startNode, string[] path)
        {
            (bool exists, ProviderNode? node) cursor = (true, startNode);

            foreach (var pathItem in path)
            {
                if (cursor.exists)
                {
                    // try to descend
                    cursor = cursor.node switch
                    {
                        // you can only fetch child node from containers
                        ContainerNode c => this.TryGetChildNode(c, pathItem),

                        // from leaf nodes, no child can be retrieved
                        _ => (false, null)
                    };

                    // the result of the last traversal is returned
                    yield return (
                        name: pathItem,
                        exists: cursor.exists,
                        node: cursor.node
                    );
                }
                else
                {
                    // path points where new node exists
                    yield return (
                        name: pathItem,
                        exists: false,
                        node: null
                    );
                }
            }
        }

        //
        //protected IEnumerable<(string name, bool exists, ProviderNode? node, T? capability)> TraversePathComplete<T>(ProviderNode startNode, string[] path) where T : class
        //{
        //    // return the start node
        //    yield return (name: string.Empty, exists: true, node: startNode, capability: startNode as T);

        //    // now traverse the path
        //    (bool exists, ProviderNode? node) cursor = (true, startNode);
        //    foreach (var pathItem in path)
        //    {
        //        if (cursor.exists)
        //        {
        //            // try to descend
        //            cursor = cursor.node switch
        //            {
        //                // you can onel fetch child node from containers
        //                ContainerNode c => this.TryGetChildNode(c, pathItem),

        //                // from leaf nodes, no child can be retrieved
        //                _ => (false, null)
        //            };

        //            // the result of the last traversal is returned
        //            yield return (
        //                name: pathItem,
        //                exists: cursor.exists,
        //                node: cursor.node,
        //                capability: cursor.node as T
        //            );
        //        }
        //        else
        //        {
        //            // path points where new node exists
        //            yield return (
        //                name: pathItem,
        //                exists: false,
        //                node: null,
        //                capability: null
        //            );
        //        }
        //    }
        // }

        #region Write a ProviderNode to pipeline

        protected void WriteProviderNode(string path, ProviderNode node)
        {
            // PowerShell would wrap the underlying with a PSObject itself
            // but taling the PSObject from the node allows to provide additionel properties
            var psobject = node.GetItem();
            if (psobject is null)
                return;

            this.WriteItemObject(
                item: psobject,
                path: this.DecoratePath(path),
                isContainer: node is ContainerNode);
        }

        protected string DecoratePath(string path) => @$"{this.PSDriveInfo.Name}:\{path}";

        #endregion Write a ProviderNode to pipeline

        #region Invoke a node capability

        public bool TryInvokeCapability<T>(ProviderNode providerNode, Action<T> invoke) where T : class
        {
            if (providerNode is T t)
            {
                invoke(t);
                return true;
            }
            return false;
        }

        #endregion Invoke a node capability
    }
}