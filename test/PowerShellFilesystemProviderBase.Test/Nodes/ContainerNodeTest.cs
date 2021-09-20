using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;
using static PowerShellFilesystemProviderBase.Test.TestData;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    public class ContainerNodeTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose() => this.mocks.VerifyAll();

        private ContainerNode ArrangeNode(string name, IServiceProvider sp) => new ContainerNode(name, sp);

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
            var getChildItem = this.mocks.Create<IGetChildItems>();
            getChildItem
                .Setup(gci => gci.GetChildItems())
                .Returns(new[] { this.ArrangeNode("name", ServiceProvider()) });

            var node = ArrangeNode("", ServiceProvider(With<IGetChildItems>(getChildItem)));

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
            var getChildItem = this.mocks.Create<IGetChildItems>();
            getChildItem
                .Setup(gci => gci.GetChildItems())
                .Returns(new[] { this.ArrangeNode("unkown", ServiceProvider()) });

            var node = ArrangeNode("", ServiceProvider(With<IGetChildItems>(getChildItem)));

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
                .Setup(gi => gi.SetItem(1));

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
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'ISetItem'.", result.Message);
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
                .Setup(ci => ci.ClearItem());

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
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IClearItem'.", result.Message);
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
                .Setup(ii => ii.ItemExists())
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
                .Setup(gi => gi.ItemExistsParameters())
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
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IInvokeItem'.", result.Message);
        }

        [Fact]
        public void InvokeItem_invokes_underlying()
        {
            // ARRANGE
            var itemExists = this.mocks.Create<IInvokeItem>();
            itemExists
                .Setup(ii => ii.InvokeItem());

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
                .Setup(gi => gi.InvokeItemParameters())
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
                ArrangeLeafNode("child1", ServiceProvider()),
                //ContainerNodeFactory.Create("child2", new Dictionary<string,object>())
            };

            var underlying = this.mocks.Create<IGetChildItems>();
            underlying
                .Setup(u => u.GetChildItems())
                .Returns(childItems);

            var node = this.ArrangeNode("name", ServiceProvider(With<IGetChildItems>(underlying)));

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Equal(childItems, result);
        }

        [Fact]
        public void GetChildItems_defaults_to_empty()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Empty(result);
        }

        [Fact]
        public void GetChildItemsParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IGetChildItems>();
            underlying
                .Setup(u => u.GetChildItemParameters("path", true))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IGetChildItems>(underlying)));

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
            var underlying = this.mocks.Create<IGetChildItems>();
            underlying
                .Setup(u => u.HasChildItems())
                .Returns(true);

            var node = this.ArrangeNode("name", ServiceProvider(With<IGetChildItems>(underlying)));

            // ACT
            var result = node.HasChildItems();

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public void HasChildItems_defaults_to_false()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.HasChildItems();

            // ASSERT
            Assert.False(result);
        }

        #endregion IGetChildItems

        #region IRemoveChildItem

        [Fact]
        public void RemoveChildItem_invokes_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<IRemoveChildItem>();
            underlying
                .Setup(u => u.RemoveChildItem("child1"));

            var node = this.ArrangeNode("name", ServiceProvider(With<IRemoveChildItem>(underlying)));

            // ACT
            node.RemoveChildItem("child1");
        }

        [Fact]
        public void RemoveChildItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.RemoveChildItem("child1"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IRemoveChildItem'.", result.Message);
        }

        [Fact]
        public void RemoveChildItemParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IRemoveChildItem>();
            underlying
                .Setup(u => u.RemoveChildItemParameters("child1", true))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IRemoveChildItem>(underlying)));

            // ACT
            var result = node.RemoveChildItemParameters("child1", true);

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void RemoveChildItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.RemoveChildItemParameters("child1", true);

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
                .Setup(u => u.NewChildItem("child1", "itemTypeName", value))
                .Returns(new LeafNode("child1", ServiceProvider()));

            var node = this.ArrangeNode("name", ServiceProvider(With<INewChildItem>(underlying)));

            // ACT
            node.NewChildItem("child1", "itemTypeName", value);
        }

        [Fact]
        public void NewChildItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.NewChildItem("child1", "itemTypeValue", null));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'INewChildItem'.", result.Message);
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
                .Setup(u => u.RenameChildItem("name", "newName"));

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
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IRenameChildItem'.", result.Message);
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
                .Setup(u => u.CopyChildItemRecursive(It.IsAny<ProviderNode>(), new[] { "child", "grandchild" }))
                .Returns(new ContainerNode("name", ServiceProvider()));

            var node = this.ArrangeNode("name", ServiceProvider(With<ICopyChildItemRecursive>(underlying)));

            // ACT
            node.CopyChildItem(new ContainerNode("name", ServiceProvider()), new[] { "child", "grandchild" }, true);
        }

        [Fact]
        public void CopyChildItem_invokes_underlying_if_non_recursive()
        {
            // ARRANGE
            var underlying = this.mocks.Create<ICopyChildItem>();
            underlying
                .Setup(u => u.CopyChildItem(It.IsAny<ProviderNode>(), new[] { "child", "grandchild" }))
                .Returns(new ContainerNode("name", ServiceProvider()));

            var node = this.ArrangeNode("name", ServiceProvider(With<ICopyChildItem>(underlying)));

            // ACT
            node.CopyChildItem(new ContainerNode("name", ServiceProvider()), new[] { "child", "grandchild" }, false);
        }

        [Fact]
        public void CopyChildItemRecursive_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.CopyChildItem(new ContainerNode("name", ServiceProvider()), new[] { "child", "grandchild" }, false));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'ICopyChildItem'.", result.Message);
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
            var parentOfNode = new ContainerNode("Name", ServiceProvider());
            var nodeToMove = new LeafNode("name", ServiceProvider());

            var underlying = this.mocks.Create<IMoveChildItem>();
            underlying
                .Setup(u => u.MoveChildItem(parentOfNode, nodeToMove, new string[0]));

            var node = this.ArrangeNode("name", ServiceProvider(With<IMoveChildItem>(underlying)));

            // ACT
            node.MoveChildItem(parentOfNode, nodeToMove, destination: new string[0]);
        }

        [Fact]
        public void MoveChildItemParameters_invokes_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IMoveChildItem>();
            underlying
                .Setup(u => u.MoveChildItemParameters("name", "destination", true))
                .Returns(parameters);

            var node = this.ArrangeNode("name", ServiceProvider(With<IMoveChildItem>(underlying)));

            // ACT
            var result = node.MoveChildItemParameter("name", "destination", true);

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void MoveChildItem_defaults_to_exception()
        {
            // ARRANGE
            var parentOfNode = new ContainerNode("Name", ServiceProvider());
            var nodeToMove = new LeafNode("name", ServiceProvider());
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.MoveChildItem(parentOfNode, nodeToMove, destination: new string[0]));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IMoveChildItem'.", result.Message);
        }

        [Fact]
        public void MoveChildItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", ServiceProvider());

            // ACT
            var result = node.MoveChildItemParameter("name", "destination", true);

            // ASSERT
            Assert.Null(result);
        }

        #endregion IMoveChildItem
    }
}