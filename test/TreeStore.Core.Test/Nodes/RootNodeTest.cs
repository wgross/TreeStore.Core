using TreeStore.Core.Nodes;
using Xunit;
using static TreeStore.Core.Test.TestData;

namespace TreeStore.Core.Test.Nodes
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