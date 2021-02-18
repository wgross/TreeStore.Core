using PowerShellFilesystemProviderBase.Nodes;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    public class RootNodeTest
    {
        [Fact]
        public void RootNode_has_empty_Name()
        {
            // ACT
            var node = new RootNode(new { });

            // ASSERT
            Assert.Equal(string.Empty, node.Name);
        }
    }
}