using PowerShellFilesystemProviderBase.Nodes;
using Xunit;
using static PowerShellFilesystemProviderBase.Test.TestData;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    public class RootNodeTest
    {
        [Fact]
        public void RootNode_has_empty_Name()
        {
            // ACT
            var node = new RootNode(ServiceProvider());

            // ASSERT
            Assert.Equal(string.Empty, node.Name);
        }
    }
}