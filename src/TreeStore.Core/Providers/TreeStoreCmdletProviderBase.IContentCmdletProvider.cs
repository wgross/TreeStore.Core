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
            if (this.TryGetNodeByPath(path, out var node))
            {
                return node.GetItemContentWriter();
            }
            else return null;
        }

        public object? GetContentWriterDynamicParameters(string path)
        {
            if (this.TryGetNodeByPath(path, out var node))
            {
                return node.SetItemContentParameters();
            }
            else return null;
        }
    }
}