using Moq;
using System;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.Core.Capabilities;
using TreeStore.Core.Nodes;
using TreeStore.Core.Providers;
using Xunit;
using static TreeStore.Core.Test.TestData;

namespace TreeStore.Core.Test
{
    public class ContainerNodeTest : IDisposable
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
        public void ContainerNode_rejects_null_data()
        {
            // ACT & ASSERT
            var node = Assert.Throws<ArgumentNullException>(() => this.ArrangeNode("name", null));
        }

        #region Name

        [Fact]
        public void ContainerNode_rejects_null_name()
        {
            // ACT & ASSERT
            var node = Assert.Throws<ArgumentNullException>(() => this.ArrangeNode(null, ServiceProvider()));
        }

        [Fact]
        public void ContainerNode_provides_name()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.Name;

            // ASSERT
            Assert.Equal("name", result);
        }

        #endregion Name

        [Theory]
        [InlineData("NAME")]
        [InlineData("name")]
        public void ContainerNode_finds_child_by_name(string name)
        {
            // ARRANGE
            var getChildItem = this.mocks.Create<IGetChildItem>();
            getChildItem
                .Setup(gci => gci.GetChildItems(this.providerMock.Object))
                .Returns(new[] { this.ArrangeNode("name", ServiceProvider()) });

            var node = this.ArrangeNode("", ServiceProvider(With<IGetChildItem>(getChildItem)));

            // ACT
            var result = node.TryGetChildNode(name, out var childNode);

            // ASSERT
            Assert.True(result);
            Assert.NotNull(childNode);
        }

        [Fact]
        public void ContainerNode_finding_unknown_child_item_by_name_returns_null()
        {
            // ARRANGE
            var getChildItem = this.mocks.Create<IGetChildItem>();
            getChildItem
                .Setup(gci => gci.GetChildItems(this.providerMock.Object))
                .Returns(new[] { this.ArrangeNode("unkown", ServiceProvider()) });

            var node = this.ArrangeNode("", ServiceProvider(With<IGetChildItem>(getChildItem)));

            // ACT
            var result = node.TryGetChildNode("name", out var childNode);

            // ASSERT
            Assert.False(result);
            Assert.Null(childNode);
        }

        #region ISetItem

        [Fact]
        public void SetItem_invokes_underlying()
        {
            // ARRANGE
            var setItem = this.mocks.Create<ISetItem>();
            setItem
                .Setup(gi => gi.SetItem(this.providerMock.Object, 1));

            var node = this.ArrangeNode("name", ServiceProvider(With<ISetItem>(setItem)));

            // ACT
            node.SetItem(1);
        }

        [Fact]
        public void SetItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.SetItem(1));

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'ISetItem'.", result.Message);
        }

        [Fact]
        public void SetItemParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var setItem = this.mocks.Create<ISetItem>();
            setItem
                .Setup(gi => gi.SetItemParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<ISetItem>(setItem)));

            // ACT
            var result = node.SetItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void SetItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.SetItemParameters();

            // ASSERT
            Assert.Null(result);
        }

        #endregion ISetItem

        #region IClearItem

        [Fact]
        public void ClearItem_invokes_underlying()
        {
            // ARRANGE
            var clearItem = this.mocks.Create<IClearItem>();
            clearItem
                .Setup(ci => ci.ClearItem(this.providerMock.Object));

            var node = this.ArrangeNode("name", ServiceProvider(With<IClearItem>(clearItem)));

            // ACT
            node.ClearItem();
        }

        [Fact]
        public void ClearItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.ClearItem());

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'IClearItem'.", result.Message);
        }

        [Fact]
        public void ClearItemParameters_invokes_Underlying()
        {
            // ARRANGE
            var parameters = new object();
            var clearItem = this.mocks.Create<IClearItem>();
            clearItem
                .Setup(gi => gi.ClearItemParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IClearItem>(clearItem)));

            // ACT
            var result = node.ClearItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void ClearItemParameter_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.ClearItemParameters();

            // ASSERT
            Assert.Null(result);
        }

        #endregion IClearItem

        #region IItemExists

        [Fact]
        public void ItemExists_defaults_to_true()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.ItemExists();

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public void ItemExists_invokes_underlying()
        {
            // ARRANGE
            var itemExists = this.mocks.Create<IItemExists>();
            itemExists
                .Setup(ii => ii.ItemExists(this.providerMock.Object))
                .Returns(false);

            var node = this.ArrangeNode("name", ServiceProvider(With<IItemExists>(itemExists)));

            // ACT
            var result = node.ItemExists();

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void ItemExistsParameter_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var itemExists = this.mocks.Create<IItemExists>();
            itemExists
                .Setup(gi => gi.ItemExistsParameters(this.providerMock.Object))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IItemExists>(itemExists)));

            // ACT
            var result = node.ItemExistsParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void ItemExistsParameter_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.ItemExistsParameters();

            // ASSERT
            Assert.Null(result);
        }

        #endregion IItemExists

        #region IInvokeItem

        [Fact]
        public void InvokeItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.InvokeItem());

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'IInvokeItem'.", result.Message);
        }

        [Fact]
        public void InvokeItem_invokes_underlying()
        {
            // ARRANGE
            var itemExists = this.mocks.Create<IInvokeItem>();
            itemExists
                .Setup(ii => ii.InvokeItem(this.providerMock.Object));

            var node = this.ArrangeNode("name", ServiceProvider(With<IInvokeItem>(itemExists)));

            // ACT
            node.InvokeItem();
        }

        [Fact]
        public void InvokeItemParameter_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var itemExists = this.mocks.Create<IInvokeItem>();
            itemExists
                .Setup(gi => gi.InvokeItemParameters(this.providerMock.Object))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IInvokeItem>(itemExists)));

            // ACT
            var result = node.InvokeItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void InvokeItemParameter_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.InvokeItemParameters();

            // ASSERT
            Assert.Null(result);
        }

        #endregion IInvokeItem

        #region IGetChildItems

        [Fact]
        public void GetChildItems_invokes_underlying()
        {
            // ARRANGE
            var childItems = new ProviderNode[]
            {
                ArrangeLeafNode(this.providerMock.Object,"child1", ServiceProvider()),
            };

            var underlying = this.mocks.Create<IGetChildItem>();
            underlying
                .Setup(u => u.GetChildItems(this.providerMock.Object))
                .Returns(childItems);

            var node = this.ArrangeNode("name", ServiceProvider(With<IGetChildItem>(underlying)));

            // ACT
            var result = node.GetChildItems(this.providerMock.Object).ToArray();

            // ASSERT
            Assert.Equal(childItems, result);
        }

        [Fact]
        public void GetChildItems_defaults_to_empty()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.GetChildItems(this.providerMock.Object).ToArray();

            // ASSERT
            Assert.Empty(result);
        }

        [Fact]
        public void GetChildItemsParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IGetChildItem>();
            underlying
                .Setup(u => u.GetChildItemParameters("path", true))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IGetChildItem>(underlying)));

            // ACT
            var result = node.GetChildItemParameters("path", recurse: true);

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void GetChildItemsParameters_defaults_to_empty()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.GetChildItemParameters("path", true);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void HasChildItems_invokes_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<IGetChildItem>();
            underlying
                .Setup(u => u.HasChildItems(this.providerMock.Object))
                .Returns(true);

            var node = this.ArrangeNode("name", ServiceProvider(With<IGetChildItem>(underlying)));

            // ACT
            var result = node.HasChildItems(this.providerMock.Object);

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public void HasChildItems_defaults_to_false()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.HasChildItems(this.providerMock.Object);

            // ASSERT
            Assert.False(result);
        }

        #endregion IGetChildItems

        #region IRemoveChildItem

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveChildItem_invokes_underlying(bool recurse)
        {
            // ARRANGE
            var underlying = this.mocks.Create<IRemoveChildItem>();
            underlying
                .Setup(u => u.RemoveChildItem(this.providerMock.Object, "child1", recurse));

            var node = this.ArrangeNode("name", ServiceProvider(With<IRemoveChildItem>(underlying)));

            // ACT
            node.RemoveChildItem("child1", recurse);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveChildItem_defaults_to_exception(bool recurse)
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.RemoveChildItem("child1", recurse));

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'IRemoveChildItem'.", result.Message);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveChildItemParameters_invokes_underlying(bool recurse)
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IRemoveChildItem>();
            underlying
                .Setup(u => u.RemoveChildItemParameters("child1", recurse))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IRemoveChildItem>(underlying)));

            // ACT
            var result = node.RemoveChildItemParameters("child1", recurse);

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RemoveChildItemParameters_defaults_to_null(bool recurse)
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.RemoveChildItemParameters("child1", recurse);

            // ASSERT
            Assert.Null(result);
        }

        #endregion IRemoveChildItem

        #region INewChildItem

        [Fact]
        public void NewChildItem_invokes_underlying()
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
        }

        [Fact]
        public void NewChildItem_fails_with_null()
        {
            // ARRANGE
            var underlying = this.mocks.Create<INewChildItem>();
            var value = new object();
            underlying
                .Setup(u => u.NewChildItem(this.providerMock.Object, "child1", "itemTypeName", value))
                .Returns(new NewChildItemResult(false, null, null));

            var node = this.ArrangeNode("name", ServiceProvider(With<INewChildItem>(underlying)));

            // ACT
            var result = node.NewChildItem("child1", "itemTypeName", value);

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void NewChildItem_defaults_to_exception()
        {
            // ARRANGE
            // service provider doesn't provide INewChildItem
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.NewChildItem("child1", "itemTypeValue", null));

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'INewChildItem'.", result.Message);
        }

        [Fact]
        public void NewChildItemParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<INewChildItem>();
            underlying
                .Setup(u => u.NewChildItemParameters("child1", "newItemTypeValue", null))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<INewChildItem>(underlying)));

            // ACT
            var result = node.NewChildItemParameters("child1", "newItemTypeValue", null);

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void NewChildItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.NewChildItemParameters("child1", "itemTypeValue", null);

            // ASSERT
            Assert.Null(result);
        }

        #endregion INewChildItem

        #region IRenameChildItem

        [Fact]
        public void RenameChildItem_invoke_underlyling()
        {
            // ARRANGE
            var underlying = this.mocks.Create<IRenameChildItem>();
            underlying
                .Setup(u => u.RenameChildItem(this.providerMock.Object, "name", "newName"));

            var node = this.ArrangeNode("name", ServiceProvider(With<IRenameChildItem>(underlying)));

            // ACT
            node.RenameChildItem("name", "newName");
        }

        [Fact]
        public void RenameChildItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.RenameChildItem("name", "newName"));

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'IRenameChildItem'.", result.Message);
        }

        [Fact]
        public void RenameChildItemParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IRenameChildItem>();
            underlying
                .Setup(u => u.RenameChildItemParameters("name", "newName"))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IRenameChildItem>(underlying)));

            // ACT
            var result = node.RenameChildItemParameters("name", "newName");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void RenameChildItemParameters_defaults_to_null()
        {
            // ARRANGE

            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.RenameChildItemParameters("name", "newName");

            // ASSERT
            Assert.Null(result);
        }

        #endregion IRenameChildItem

        #region ICopyChildItemRecursive

        [Fact]
        public void CopyChildItemRecursive_invokes_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<ICopyChildItemRecursive>();
            underlying
                .Setup(u => u.CopyChildItemRecursive(this.providerMock.Object, It.IsAny<ProviderNode>(), new[] { "child", "grandchild" }))
                .Returns(new CopyChildItemResult(true, "name", ServiceProvider()));

            var node = this.ArrangeNode("name", ServiceProvider(With<ICopyChildItemRecursive>(underlying)));

            // ACT
            node.CopyChildItem(new ContainerNode(this.providerMock.Object, "name", ServiceProvider()), new[] { "child", "grandchild" }, true);
        }

        [Fact]
        public void CopyChildItem_invokes_underlying_if_non_recursive()
        {
            // ARRANGE
            var underlying = this.mocks.Create<ICopyChildItem>();
            underlying
                .Setup(u => u.CopyChildItem(this.providerMock.Object, It.IsAny<ProviderNode>(), new[] { "child", "grandchild" }))
                .Returns(new CopyChildItemResult(true, "name", ServiceProvider()));

            var node = this.ArrangeNode("name", ServiceProvider(With<ICopyChildItem>(underlying)));

            // ACT
            node.CopyChildItem(new ContainerNode(this.providerMock.Object, "name", ServiceProvider()), new[] { "child", "grandchild" }, false);
        }

        [Fact]
        public void CopyChildItemRecursive_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.CopyChildItem(new ContainerNode(this.providerMock.Object, "name", ServiceProvider()), new[] { "child", "grandchild" }, false));

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'ICopyChildItem'.", result.Message);
        }

        [Fact]
        public void CopyItemParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<ICopyChildItem>();
            underlying
                .Setup(u => u.CopyChildItemParameters("name", "destination", true))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<ICopyChildItem>(underlying)));

            // ACT
            var result = node.CopyChildItemParameters(childName: "name", destination: "destination", recurse: true);

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void CopyItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.CopyChildItemParameters(childName: "name", destination: "destination", recurse: true);

            // ASSERT
            Assert.Null(result);
        }

        #endregion ICopyChildItemRecursive

        #region IMoveChildItem

        [Fact]
        public void MoveChildItem_invokes_underlying()
        {
            // ARRANGE
            var parentOfNode = new ContainerNode(this.providerMock.Object, "Name", ServiceProvider());
            var nodeToMove = new LeafNode(this.providerMock.Object, "name", ServiceProvider());
            var destination = Array.Empty<string>();
            var underlying = this.mocks.Create<IMoveChildItem>();
            underlying
                .Setup(u => u.MoveChildItem(this.providerMock.Object, parentOfNode, nodeToMove, destination))
                .Returns(new MoveChildItemResult(true, "name", ServiceProvider()));

            var node = this.ArrangeNode("name", ServiceProvider(With<IMoveChildItem>(underlying)));

            // ACT
            node.MoveChildItem(parentOfNode, nodeToMove, destination);
        }

        [Fact]
        public void MoveChildItemParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IMoveChildItem>();
            underlying
                .Setup(u => u.MoveChildItemParameters("name", "destination"))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IMoveChildItem>(underlying)));

            // ACT
            var result = node.MoveChildItemParameters("name", "destination");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void MoveChildItem_defaults_to_exception()
        {
            // ARRANGE
            var parentOfNode = new ContainerNode(this.providerMock.Object, "Name", ServiceProvider());
            var nodeToMove = new LeafNode(this.providerMock.Object, "name", ServiceProvider());
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.MoveChildItem(parentOfNode, nodeToMove, destination: Array.Empty<string>()));

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'IMoveChildItem'.", result.Message);
        }

        [Fact]
        public void MoveChildItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.MoveChildItemParameters("name", "destination");

            // ASSERT
            Assert.Null(result);
        }

        #endregion IMoveChildItem

        #region IClearItemContent

        [Fact]
        public void ClearItemContent_invokes_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<IClearItemContent>();
            underlying
              .Setup(u => u.ClearItemContent(this.providerMock.Object));

            var node = this.ArrangeNode("name", ServiceProvider(With<IClearItemContent>(underlying)));

            // ACT
            node.ClearItemContent();
        }

        [Fact]
        public void ClearItemContentParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IClearItemContent>();
            underlying
                .Setup(u => u.ClearItemContentParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IClearItemContent>(underlying)));

            // ACT
            node.ClearItemContentParameters();
        }

        [Fact]
        public void ClearItemContent_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.ClearItemContent());

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'IClearItemContent'.", result.Message);
        }

        [Fact]
        public void ClearItemContentParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.ClearItemContentParameters();

            // ASSEERT
            Assert.Null(result);
        }

        #endregion IClearItemContent

        #region IGetItemContent

        [Fact]
        public void GetItemContent_invokes_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<IGetItemContent>();
            var contentReader = Mock.Of<IContentReader>();
            underlying
              .Setup(u => u.GetItemContentReader(this.providerMock.Object))
              .Returns(contentReader);

            var node = this.ArrangeNode("name", ServiceProvider(With<IGetItemContent>(underlying)));

            // ACT
            var result = node.GetItemContentReader();

            // ASSERT
            Assert.Same(contentReader, result);
        }

        [Fact]
        public void GetItemContentParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IGetItemContent>();
            underlying
                .Setup(u => u.GetItemContentParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IGetItemContent>(underlying)));

            // ACT
            node.GetItemContentParameters();
        }

        [Fact]
        public void GetItemContent_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.GetItemContentReader());

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'IGetItemContent'.", result.Message);
        }

        [Fact]
        public void GetItemContentParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.GetItemContentParameters();

            // ASSEERT
            Assert.Null(result);
        }

        #endregion IGetItemContent

        #region ISetItemContent

        [Fact]
        public void SetItemContent_invokes_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<ISetChildItemContent>();
            var contentReader = Mock.Of<IContentWriter>();
            underlying
              .Setup(u => u.GetChildItemContentWriter(this.providerMock.Object, "childName"))
              .Returns(contentReader);

            var node = this.ArrangeNode("name", ServiceProvider(With<ISetChildItemContent>(underlying)));

            // ACT
            var result = node.GetChildItemContentWriter("childName");

            // ASSERT
            Assert.Same(contentReader, result);
        }
        
        [Fact]
        public void SetItemContentParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<ISetChildItemContent>();
            underlying
                .Setup(u => u.SetChildItemContentParameters("childName"))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<ISetChildItemContent>(underlying)));

            // ACT
            node.SetChildItemContentParameters("childName");
        }

        [Fact]
        public void SetItemContent_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.GetChildItemContentWriter("childName"));

            // ASSERT
            Assert.Equal("Node(name='name') doesn't provide an implementation of capability 'ISetChildItemContent'.", result.Message);
        }

        [Fact]
        public void SetItemContentParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.SetChildItemContentParameters("childName");

            // ASSEERT
            Assert.Null(result);
        }

        #endregion ISetItemContent
    }
}