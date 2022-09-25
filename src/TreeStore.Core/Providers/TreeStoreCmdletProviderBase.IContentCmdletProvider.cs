using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers
{
    public partial class TreeStoreCmdletProviderBase : IContentCmdletProvider
    {
        public void ClearContent(string path)
        {
            if (this.TryGetNodeByPath(path, out var node))
            {
                node.ClearItemContent();
            }
        }

        public object? ClearContentDynamicParameters(string path)
        {
            if (this.TryGetNodeByPath(path, out var node))
            {
                return node.ClearItemContentParameters();
            }
            else return null;
        }

        public IContentReader? GetContentReader(string path)
        {
            if (this.TryGetNodeByPath(path, out var node))
            {
                return node.GetItemContentReader();
            }
            else return null;
        }

        public object? GetContentReaderDynamicParameters(string path)
        {
            if (this.TryGetNodeByPath(path, out var node))
            {
                return node.GetItemContentParameters();
            }
            else return null;
        }

        public IContentWriter? GetContentWriter(string path)
        {
            var (parentPath, childName) = new PathTool().SplitProviderQualifiedPath(path).ParentAndChild;

            if (this.TryGetNodeByPath(parentPath, out var parentNode))
            {
                if (parentNode is ContainerNode parentContainer)
                {
                    return parentContainer.GetChildItemContentWriter(childName!);
                }
            }

            return null;
        }

        public object? GetContentWriterDynamicParameters(string path)
        {
            var (parentPath, childName) = new PathTool().SplitProviderQualifiedPath(path).ParentAndChild;

            if (this.TryGetNodeByPath(parentPath, out var parentNode))
            {
                if (parentNode is ContainerNode parentContainer)
                {
                    return parentContainer.SetChildItemContentParameters(childName!);
                }
            }
            return null;
        }
    }
}