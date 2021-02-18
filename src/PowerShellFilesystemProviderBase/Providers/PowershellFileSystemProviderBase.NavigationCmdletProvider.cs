using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System.Linq;

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
                parentPath = $"{splittedProviderPath.drive}:\\{parentPath}";
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
                if (node is ContainerNode)
                    return true;
            return false;
        }

        protected override void MoveItem(string path, string destination)
        {
            var (parentPath, childName) = new PathTool().SplitParentPathAndChildName(path);

            if (this.TryGetNodeByPath<IGetChildItems>(this.DriveInfo.Root, out var sourceParentNode, out var getChildItem))
            {
                // TODO: name comparision is responsibility of the node -> TryGetChildItem(name)?!
                var nodeToMove = getChildItem.GetChildItems().Single(ci => ci.Name.Equals(childName));

                var destinationAncestor = this.GetDeepestNodeByPath(destination, out var missingPath);

                // the parent exists, try to create a new child as a copy.
                if (TryInvokeCapability<IMoveChildItem>(destinationAncestor, c => c.MoveChildItem((ContainerNode)sourceParentNode, nodeToMove, missingPath)))
                    return;
            }
            else base.MoveItem(path, destination); // fallback: NotSupported
        }

        protected override object MoveItemDynamicParameters(string path, string destination)
        {
            return base.MoveItemDynamicParameters(path, destination);
        }

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