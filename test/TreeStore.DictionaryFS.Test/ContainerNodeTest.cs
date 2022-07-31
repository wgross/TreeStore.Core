using Moq;
using System;
using TreeStore.Core.Capabilities;
using TreeStore.Core.Nodes;
using TreeStore.Core.Providers;
using Xunit;

using static TreeStore.Core.Test.TestData;

namespace TreeStore.DictionaryFS.Test;

public class ContainerNodeTest
{
    private readonly MockRepository mocks = new(MockBehavior.Strict);
    private readonly Mock<ICmdletProvider> providerMock;

    public ContainerNodeTest()
    {
        this.providerMock = this.mocks.Create<ICmdletProvider>();
    }

    public void Dispose() => this.mocks.VerifyAll();

    private ContainerNode ArrangeNode(string name, IServiceProvider sp) => new(this.providerMock.Object, name, sp);

    [Fact]
    public void NewChildItem_invokes_underlying_for_leaf_node()
    {
        // ARRANGE
        var underlying = this.mocks.Create<INewChildItem>();
        var value = new object();
        underlying
            .Setup(u => u.NewChildItem(this.providerMock.Object, "child1", "itemTypeName", value))
            .Returns(new NewChildItemResult(true, "child1", ServiceProvider()));

        var node = this.ArrangeNode("name", ServiceProvider(With<INewChildItem>(underlying)));

        // ACT
        var result = node.NewChildItem("child1", "itemTypeName", value);

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<LeafNode>(result);
    }

    [Fact]
    public void NewChildItem_invokes_underlying_for_container_node()
    {
        // ARRANGE
        var underlying = this.mocks.Create<INewChildItem>();
        var underlying2 = this.mocks.Create<IGetChildItem>();
        underlying2
            .Setup(u => u.HasChildItems(this.providerMock.Object))
            .Returns(true);

        var value = new object();
        underlying
            .Setup(u => u.NewChildItem(this.providerMock.Object, "child1", "itemTypeName", value))
            .Returns(new NewChildItemResult(true, "child1", ServiceProvider(With<IGetChildItem>(underlying2))));

        var node = this.ArrangeNode("name", ServiceProvider(With<INewChildItem>(underlying)));

        // ACT
        var result = node.NewChildItem("child1", "itemTypeName", value);

        // ASSERT
        Assert.NotNull(result);
        Assert.IsType<ContainerNode>(result);
    }
}