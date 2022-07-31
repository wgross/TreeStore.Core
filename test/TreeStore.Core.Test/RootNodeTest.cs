using Moq;
using TreeStore.Core.Nodes;
using TreeStore.Core.Providers;
using Xunit;
using static TreeStore.Core.Test.TestData;

namespace TreeStore.Core.Test;

public class RootNodeTest
{
    private readonly MockRepository mocks = new(MockBehavior.Strict);
    private readonly Mock<ICmdletProvider> providerMock;

    public RootNodeTest()
    {
        this.providerMock = this.mocks.Create<ICmdletProvider>();
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