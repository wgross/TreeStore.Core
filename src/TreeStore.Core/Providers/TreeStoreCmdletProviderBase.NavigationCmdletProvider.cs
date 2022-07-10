using System;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers
{
    public partial class TreeStoreCmdletProviderBase
    {
        protected override string GetParentPath(string path, string root) => base.GetParentPath(path, root);

        protected override string GetChildName(string path) => base.GetChildName(path);

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

        protected override object? MoveItemDynamicParameters(string path, string destination)
            => this.InvokeContainerNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: c => c.MoveChildItemParameter(path, destination),
                fallback: () => base.MoveItemDynamicParameters(path, destination));

        protected override string MakePath(string parent, string child) => base.MakePath(parent, child);

        protected override string NormalizeRelativePath(string path, string basePath) => base.NormalizeRelativePath(path, basePath);
    }
}