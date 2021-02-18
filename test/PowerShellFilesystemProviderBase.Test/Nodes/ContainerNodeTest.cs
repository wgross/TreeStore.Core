using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    public class ContainerNodeTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose() => this.mocks.VerifyAll();

        private ContainerNode ArrangeNode(string name, object data) => new ContainerNode(name, data);

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
            var node = Assert.Throws<ArgumentNullException>(() => this.ArrangeNode(null, new { }));
        }

        [Fact]
        public void ContainerNode_provides_name()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.Name;

            // ASSERT
            Assert.Equal("name", result);
        }

        #endregion Name

        #region IContainerItem

        [Fact]
        public void ContainerNode_is_container()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", Mock.Of<IItemContainer>());

            // ACT
            var result = node.IsContainer;

            // ASSERT
            Assert.True(result);
        }

        //[Fact(Skip = "TryGetChildNode is currently retired")]
        //public void ContainerNode_finds_child_IItemContainer_by_name()
        //{
        //    // ARRANGE
        //    var node = ContainerNodeFactory.Create("name", new Dictionary<string, object>
        //    {
        //        { "container", Mock.Of<IItemContainer>() }
        //    });

        //    // ACT
        //    var result = node.TryGetChildNode("container");

        //    // ASSERT
        //    Assert.True(result.exists);
        //    Assert.Equal("container", result.node.Name);
        //    Assert.True(result.node.IsContainer);
        //}

        //[Fact]
        //public void ContainerNode_finding_unknown_child_item_by_name_returns_null()
        //{
        //    // ARRANGE
        //    var node = ContainerNodeFactory.Create("name", new Dictionary<string, object>
        //    {
        //        { "leaf", new { } }
        //    });

        //    // ACT
        //    var result = node.TryGetChildNode("unknown");

        //    // ASSERT
        //    Assert.Equal((false, default), result);
        //}

        #endregion IContainerItem

        #region IGetItem

        [Fact]
        public void Invoke_GetItem_at_Underlying()
        {
            // ARRANGE
            var psObject = new PSObject();
            var getItem = this.mocks.Create<IGetItem>();
            getItem
                .Setup(gi => gi.GetItem())
                .Returns(psObject);

            var node = this.ArrangeNode("name", getItem.Object);

            // ACT
            var result = node.GetItem();

            // ASSERT
            Assert.Same(psObject, result);
            Assert.Equal("name", psObject.Property<string>("PSChildName"));
        }

        [Fact]
        public void Invoke_GetItem_default_to_PSObject_of_underlying()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new
            {
                Data = "data"
            });

            // ACT
            var result = node.GetItem();

            // ASSERT
            Assert.Equal("name", result.Property<string>("PSChildName"));
            Assert.Equal("data", result.Property<string>("Data"));
        }

        [Fact]
        public void Invoke_GetItemParameters_at_Underlying()
        {
            // ARRANGE
            var parameters = new object();
            var getItem = this.mocks.Create<IGetItem>();
            getItem
                .Setup(gi => gi.GetItemParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", getItem.Object);

            // ACT
            var result = node.GetItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_GetItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new
            {
                Data = "data"
            });

            // ACT
            var result = node.GetItemParameters();

            // ASSERT
            Assert.Null(result);
        }

        #endregion IGetItem

        #region ISetItem

        [Fact]
        public void Invoke_SetItem_at_Underlying()
        {
            // ARRANGE
            var setItem = this.mocks.Create<ISetItem>();
            setItem
                .Setup(gi => gi.SetItem(1));

            var node = this.ArrangeNode("name", setItem.Object);

            // ACT
            node.SetItem(1);
        }

        [Fact]
        public void Invoking_SetItem_at_Underlying_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.SetItem(1));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'ISetItem'.", result.Message);
        }

        [Fact]
        public void Invoke_SetItemParameters_at_Underlying()
        {
            // ARRANGE
            var parameters = new object();
            var setItem = this.mocks.Create<ISetItem>();
            setItem
                .Setup(gi => gi.SetItemParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", setItem.Object);

            // ACT
            var result = node.SetItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_SetItemParameters_at_default_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.SetItemParameters();

            // ASSERT
            Assert.Null(result);
        }

        #endregion ISetItem

        #region IClearItem

        [Fact]
        public void Invoke_ClearItem_at_Underlying()
        {
            // ARRANGE
            var clearItem = this.mocks.Create<IClearItem>();
            clearItem
                .Setup(ci => ci.ClearItem());

            var node = this.ArrangeNode("name", clearItem.Object);

            // ACT
            node.ClearItem();
        }

        [Fact]
        public void Invoking_ClearItem_at_Underlying_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.ClearItem());

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IClearItem'.", result.Message);
        }

        [Fact]
        public void Invoke_ClearItemParameters_at_Underlying()
        {
            // ARRANGE
            var parameters = new object();
            var clearItem = this.mocks.Create<IClearItem>();
            clearItem
                .Setup(gi => gi.ClearItemParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", clearItem.Object);

            // ACT
            var result = node.ClearItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_ClearItemParameter_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.ClearItemParameters();

            // ASSERT
            Assert.Null(result);
        }

        #endregion IClearItem

        #region IItemExists

        [Fact]
        public void Invoke_ItemExists_returns_true_by_default()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new
            {
                Data = "data"
            });

            // ACT
            var result = node.ItemExists();

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public void Invoke_ItemExists_invokes_underlying()
        {
            // ARRANGE
            var itemExists = this.mocks.Create<IItemExists>();
            itemExists
                .Setup(ii => ii.ItemExists())
                .Returns(false);

            var node = this.ArrangeNode("name", itemExists.Object);

            // ACT
            var result = node.ItemExists();

            // ASSERT
            Assert.False(result);
        }

        [Fact]
        public void Invoke_ItemExistsParameter_at_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var itemExists = this.mocks.Create<IItemExists>();
            itemExists
                .Setup(gi => gi.ItemExistsParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", itemExists.Object);

            // ACT
            var result = node.ItemExistsParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_ItemExistsParameter_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.ItemExistsParameters();

            // ASSERT
            Assert.Null(result);
        }

        #endregion IItemExists

        #region IInvokeItem

        [Fact]
        public void Invoke_InvokeItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.InvokeItem());

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IInvokeItem'.", result.Message);
        }

        [Fact]
        public void Invoke_InvokeItem_invokes_underlying()
        {
            // ARRANGE
            var itemExists = this.mocks.Create<IInvokeItem>();
            itemExists
                .Setup(ii => ii.InvokeItem());

            var node = this.ArrangeNode("name", itemExists.Object);

            // ACT
            node.InvokeItem();
        }

        [Fact]
        public void Invoke_InvokeItemParameter_at_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var itemExists = this.mocks.Create<IInvokeItem>();
            itemExists
                .Setup(gi => gi.InvokeItemParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", itemExists.Object);

            // ACT
            var result = node.InvokeItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_InvokeItemParameter_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.InvokeItemParameters();

            // ASSERT
            Assert.Null(result);
        }

        #endregion IInvokeItem

        #region IGetChildItems

        [Fact]
        public void Invoke_GetChildItems_at_underlying()
        {
            // ARRANGE
            var childItems = new ProviderNode[]
            {
                LeafNodeFactory.Create("child1", new { }),
                //ContainerNodeFactory.Create("child2", new Dictionary<string,object>())
            };

            var underlying = this.mocks.Create<IGetChildItems>();
            underlying
                .Setup(u => u.GetChildItems())
                .Returns(childItems);

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Equal(childItems, result);
        }

        [Fact]
        public void Invoke_GetChildItems_defaults_to_empty()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Empty(result);
        }

        [Fact]
        public void Invoke_GetChildItemsParameters_at_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IGetChildItems>();
            underlying
                .Setup(u => u.GetChildItemParameters())
                .Returns(parameters);

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            var result = node.GetChildItemParameters();

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_GetChildItemsParameters_defaults_to_empty()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.GetChildItemParameters();

            // ASSERT
            Assert.Null(result);
        }

        [Fact]
        public void Invoke_HasChildItems_at_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<IGetChildItems>();
            underlying
                .Setup(u => u.HasChildItems())
                .Returns(true);

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            var result = node.HasChildItems();

            // ASSERT
            Assert.True(result);
        }

        [Fact]
        public void Invoke_HasChildItems_default_to_false()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.HasChildItems();

            // ASSERT
            Assert.False(result);
        }

        #endregion IGetChildItems

        #region IRemoveChildItem

        [Fact]
        public void Invoke_RemoveChildItem_at_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<IRemoveChildItem>();
            underlying
                .Setup(u => u.RemoveChildItem("child1"));

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            node.RemoveChildItem("child1");
        }

        [Fact]
        public void Invoke_RemoveChildItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.RemoveChildItem("child1"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IRemoveChildItem'.", result.Message);
        }

        [Fact]
        public void Invoke_RemoveChildItemParameters_at_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IRemoveChildItem>();
            underlying
                .Setup(u => u.RemoveChildItemParameters("child1"))
                .Returns(parameters);

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            var result = node.RemoveChildItemParameters("child1");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_RemoveChildItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.RemoveChildItemParameters("child1");

            // ASSERT
            Assert.Null(result);
        }

        #endregion IRemoveChildItem

        #region INewChildItem

        [Fact]
        public void Invoke_NewChildItem_at_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<INewChildItem>();
            var value = new object();
            underlying
                .Setup(u => u.NewChildItem("child1", "itemTypeName", value))
                .Returns(ProviderNodeFactory.Create("child1", new { }));

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            node.NewChildItem("child1", "itemTypeName", value);
        }

        [Fact]
        public void Invoke_NewChildItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.NewChildItem("child1", "itemTypeValue", null));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'INewChildItem'.", result.Message);
        }

        [Fact]
        public void Invoke_NewChildItemParameters_at_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<INewChildItem>();
            underlying
                .Setup(u => u.NewChildItemParameters("child1", "newItemTypeValue", null))
                .Returns(parameters);

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            var result = node.NewChildItemParameters("child1", "newItemTypeValue", null);

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_NewChildItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.NewChildItemParameters("child1", "itemTypeValue", null);

            // ASSERT
            Assert.Null(result);
        }

        #endregion INewChildItem

        #region IRenameChildItem

        [Fact]
        public void Invoke_RenameChildItem_at_underlyling()
        {
            // ARRANGE
            var underlying = this.mocks.Create<IRenameChildItem>();
            underlying
                .Setup(u => u.RenameChildItem("name", "newName"));

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            node.RenameChildItem("name", "newName");
        }

        [Fact]
        public void Invoke_RenameChildItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.RenameChildItem("name", "newName"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IRenameChildItem'.", result.Message);
        }

        [Fact]
        public void Invoke_RenameChildItemParameters_at_underlyling()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<IRenameChildItem>();
            underlying
                .Setup(u => u.RenameChildItemParameters("name", "newName"))
                .Returns(parameters);

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            var result = node.RenameChildItemParameters("name", "newName");

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_RenameChildItemParameters_defaults_to_null()
        {
            // ARRANGE

            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.RenameChildItemParameters("name", "newName");

            // ASSERT
            Assert.Null(result);
        }

        #endregion IRenameChildItem

        #region ICopyChildItem

        [Fact]
        public void Invoke_CopyChildItem_at_underlying()
        {
            // ARRANGE
            var underlying = this.mocks.Create<ICopyChildItem>();
            underlying
                .Setup(u => u.NewChildItemAsCopy("name", "newName"));

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            node.NewChildItemAsCopy(childName: "name", newItemValue: "newName");
        }

        [Fact]
        public void Invoke_CopyChildItem_defaults_to_exception()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.NewChildItemAsCopy(childName: "name", newItemValue: "newName"));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'ICopyChildItem'.", result.Message);
        }

        [Fact]
        public void Invoke_CopyItemParameters_at_underlying()
        {
            // ARRANGE
            var parameters = new object();
            var underlying = this.mocks.Create<ICopyChildItem>();
            underlying
                .Setup(u => u.CopyChildItemParameters("name", "destination", true))
                .Returns(parameters);

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            var result = node.CopyChildItemParameters(childName: "name", destination: "destination", recurse: true);

            // ASSERT
            Assert.Same(parameters, result);
        }

        [Fact]
        public void Invoke_CopyItemParameters_defaults_to_null()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.CopyChildItemParameters(childName: "name", destination: "destination", recurse: true);

            // ASSERT
            Assert.Null(result);
        }

        #endregion ICopyChildItem

        #region IMoveChildItem

        [Fact]
        public void Invoke_MoveChildItem_at_underlying()
        {
            // ARRANGE
            var parentOfNode = new ContainerNode("Name", new { });
            var nodeToMove = new LeafNode("name", new { });

            var underlying = this.mocks.Create<IMoveChildItem>();
            underlying
                .Setup(u => u.MoveChildItem(parentOfNode, nodeToMove, new string[0]));

            var node = this.ArrangeNode("name", underlying.Object);

            // ACT
            node.MoveChildItem(parentOfNode, nodeToMove, destination: new string[0]);
        }

        [Fact]
        public void Invoke_MoveChildItem_defaults_to_exception()
        {
            // ARRANGE
            var parentOfNode = new ContainerNode("Name", new { });
            var nodeToMove = new LeafNode("name", new { });
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.MoveChildItem(parentOfNode, nodeToMove, destination: new string[0]));

            // ASSERT
            Assert.Equal($"Node(name='name') doesn't provide an implementation of capability 'IMoveChildItem'.", result.Message);
        }

        #endregion IMoveChildItem
    }
}