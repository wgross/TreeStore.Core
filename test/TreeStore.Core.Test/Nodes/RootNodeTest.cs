using Moq;
using System.Management.Automation.Provider;
using TreeStore.Core.Nodes;
using Xunit;
using static TreeStore.Core.Test.TestData;

namespace TreeStore.Core.Test.Nodes
{
    public class RootNodeTest
    {
        private readonly MockRepository mocks = new(MockBehavior.Strict);
        private readonly Mock<CmdletProvider> providerMock;

        public RootNodeTest()
        {
            this.providerMock = this.mocks.Create<CmdletProvider>();
        }

        [Fact]
        public void RootNode_has_empty_Name()
        {
            // ACT
            var node = new RootNode(this.providerMock.Object, ServiceProvider());

            // ASSERT
            Assert.Equal(string.Empty, node.Name);
        }
    }
}