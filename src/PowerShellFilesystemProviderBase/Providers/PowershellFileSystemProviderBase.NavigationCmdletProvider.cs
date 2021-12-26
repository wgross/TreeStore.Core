using PowerShellFilesystemProviderBase.Nodes;
using System;

namespace PowerShellFilesystemProviderBase.Providers
{
    public partial class PowerShellFileSystemProviderBase
    {
        protected override string GetParentPath(string path, string root)
        {
            var splittedProviderPath = new PathTool().SplitProviderPath(path);
            var splittedPath = new PathTool().SplitParentPathAndChildName(path);

            var parentPath = string.Join(this.ItemSeparator, splittedPath.parentPath);

            if (splittedProviderPath.path.isRooted)
            {
                if (splittedProviderPath.path.items.Length > 0)
                {
                    parentPath = $"{splittedProviderPath.drive}:\\{parentPath}";
                }
                else
                {
                    // the root node has no parent
                    return string.Empty;
                }
            }

            if (!string.IsNullOrEmpty(splittedProviderPath.module))
            {
                parentPath = $"{splittedProviderPath.module}\\{splittedProviderPath.provider}::{parentPath}";
            }

            return parentPath;
        }

        protected override bool IsItemContainer(string path)
        {
            if (this.TryGetNodeByPath(path, out var node))
            {
                if (node is ContainerNode)
                {
                    return true;
                }
            }

            return false;
        }

        protected override void MoveItem(string path, string destination)
        {
            var (parentPath, childName) = new PathTool().SplitParentPathAndChildName(path);

            this.InvokeContainerNodeOrDefault(
                path: parentPath,
                invoke: sourceParentNode =>
                {
                    if (!sourceParentNode.TryGetChildNode(childName!, out var nodeToMove))
                        throw new InvalidOperationException($"Item '{path}' doesn't exist");

                    // find the deepest ancestor which serves as a destination to copy to
                    var destinationAncestor = this.GetDeepestNodeByPath(destination, out var missingPath);

                    if (destinationAncestor is ContainerNode destinationAncestorContainer)
                    {
                        // destination ancestor is a container and might accept the move operation
                        destinationAncestorContainer.MoveChildItem(sourceParentNode, nodeToMove, destination: missingPath);
                    }
                    else
                    {
                        base.MoveItem(path, destination);
                    }
                },
                fallback: () => base.MoveItem(path, destination));
        }

        protected override object MoveItemDynamicParameters(string path, string destination)
            => this.InvokeContainerNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: c => c.MoveChildItemParameter(path, destination),
                fallback: () => base.MoveItemDynamicParameters(path, destination));

        protected override string MakePath(string parent, string child)
        {
            return base.MakePath(parent, child);
        }

        protected override string NormalizeRelativePath(string path, string basePath)
        {
            return base.NormalizeRelativePath(path, basePath);
        }
    }
}