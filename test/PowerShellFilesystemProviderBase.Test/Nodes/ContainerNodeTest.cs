using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
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

        [Fact]
        public void ContainerNode_finds_child_dictionary_container_by_name()
        {
            // ARRANGE
            var node = ContainerNodeFactory.Create("name", new Dictionary<string, object>
            {
                { "container", new Dictionary<string,DateTime> { } }
            });

            // ACT
            var result = node.TryGetChildNode("container");

            // ASSERT
            Assert.True(result.exists);
            Assert.Equal("container", result.node.Name);
            Assert.True(result.node.IsContainer);
        }

        [Fact]
        public void ContainerNode_finds_child_IItemContainer_by_name()
        {
            // ARRANGE
            var node = ContainerNodeFactory.Create("name", new Dictionary<string, object>
            {
                { "container", Mock.Of<IItemContainer>() }
            });

            // ACT
            var result = node.TryGetChildNode("container");

            // ASSERT
            Assert.True(result.exists);
            Assert.Equal("container", result.node.Name);
            Assert.True(result.node.IsContainer);
        }

        [Fact]
        public void ContainerNode_TryGetchildItem_ignores_data_property()
        {
            // ARRANGE
            var node = ContainerNodeFactory.Create("name", new Dictionary<string, object>
            {
                { "data", new { } }
            });

            // ACT
            var result = node.TryGetChildNode("data");

            // ASSERT
            Assert.False(result.exists);
        }

        [Fact]
        public void ContainerNode_finds_child_leaf_by_name()
        {
            // ARRANGE
            var node = ContainerNodeFactory.Create("name", new Dictionary<string, object>
            {
                { "leaf", new { } } // ToDo: is this a leaf or a property value?
            });

            // ACT
            var result = node.TryGetChildNode("leaf");

            // ASSERT
            Assert.True(result.exists);
            Assert.Equal("leaf", result.node.Name);
            Assert.False(result.node.IsContainer);
        }

        [Fact]
        public void ContainerNode_finding_unknown_child_item_by_name_returns_null()
        {
            // ARRANGE
            var node = ContainerNodeFactory.Create("name", new Dictionary<string, object>
            {
                { "leaf", new { } }
            });

            // ACT
            var result = node.TryGetChildNode("unknown");

            // ASSERT
            Assert.Equal((false, default), result);
        }

        public class ContainerData : IItemContainer
        {
            public (bool exists, ProviderNode node) TryGetChildNode(string name)
            {
                return (true, ProviderNodeFactory.Create(name, new { }));
            }
        }

        [Fact]
        public void ContainerNode_finds_child_leaf_data_by_name()
        {
            // ARRANGE
            var node = ContainerNodeFactory.CreateFromIItemContainer("name", new ContainerData());

            // ACT
            var result = node.TryGetChildNode("child");

            // ASSERT
            Assert.True(result.exists);
            Assert.Equal("child", result.node.Name);
            Assert.False(result.node.IsContainer);
        }

        #endregion IContainerItem

        #region IGetItem

        [Fact]
        public void Invoke_GetItem_creates_PSObject_of_underlying()
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
        public void Invoke_GetItemParameters_creates_empty_parameters()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new
            {
                Data = "data"
            });

            // ACT
            var result = node.GetItemParameters();

            // ASSERT
            Assert.Empty((RuntimeDefinedParameterDictionary)result);
        }

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
        public void Invoking_SetItem_at_Underlying_throws_if_not_supported()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.SetItem(1));

            // ASSERT
            Assert.Equal("Item 'name' can't be set", result.Message);
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
        public void Invoking_ClearItem_at_Underlying_throws_if_not_supported()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = Assert.Throws<PSNotSupportedException>(() => node.ClearItem());

            // ASSERT
            Assert.Equal("Item 'name' can't be cleared", result.Message);
        }

        [Fact]
        public void Invoke_ClearItemParameter_returns_empty_by_default()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.ClearItemParameters();

            // ASSERT
            Assert.Empty((RuntimeDefinedParameterDictionary)result);
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
        public void Invoke_ItemExistsParameter_returns_empty_by_default()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.ItemExistsParameters();

            // ASSERT
            Assert.Empty((RuntimeDefinedParameterDictionary)result);
        }

        #endregion IItemExists

        #region IInvokeItem

        [Fact]
        public void Invoke_InvokeItem_does_nothing_default()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new
            {
                Data = "data"
            });

            // ACT
            node.InvokeItem();
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
        public void Invoke_InvokeItemParameter_returns_empty_by_default()
        {
            // ARRANGE
            var node = this.ArrangeNode("name", new { });

            // ACT
            var result = node.InvokeItemParameters();

            // ASSERT
            Assert.Empty((RuntimeDefinedParameterDictionary)result);
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
                ContainerNodeFactory.Create("child2", new Dictionary<string,object>())
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
        public void Invoke_GetChildItemsParameters_at_underlying()
        {
            // ARRANGE
            var childItems = new ProviderNode[]
            {
                LeafNodeFactory.Create("child1", new { }),
                ContainerNodeFactory.Create("child2", new Dictionary<string,object>())
            };

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
        public void Invoke_GetChildItems_from_dictionary_container_by_default()
        {
            // ARRANGE

            var underlying = new Dictionary<string, object>
            {
                { "container1", new Dictionary<string, object> { { "leaf", new { } } } },
                { "property" , "text" },
                { "container2", Mock.Of<IItemContainer>() },
            };

            var node = this.ArrangeNode("name", new DictionaryContainerNode<Dictionary<string, object>, object>(underlying));

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Equal(2, result.Count());
            Assert.Equal(new[] { "container1", "container2" }, result.Select(n => n.Name));
            Assert.All(result, r => Assert.True(r.IsContainer));
        }

        [Fact]
        public void Invoke_GetChildItemParameters_from_dictionary_container_by_default()
        {
            // ARRANGE

            var underlying = new Dictionary<string, object>
            {
                { "container1", new Dictionary<string, object> { { "leaf", new { } } } },
                { "property" , "text" },
                { "container2", Mock.Of<IItemContainer>() },
            };

            var node = this.ArrangeNode("name", new DictionaryContainerNode<Dictionary<string, object>, object>(underlying));

            // ACT
            var result = node.GetChildItemParameters();

            // ASSERT
            Assert.Empty((RuntimeDefinedParameterDictionary)result);
        }

        [Fact]
        public void Invoke_GetChildItems_from_empty_container_is_empty()
        {
            // ARRANGE

            var underlying = new Dictionary<string, object>();
            var node = this.ArrangeNode("name", new DictionaryContainerNode<Dictionary<string, object>, object>(underlying));

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Empty(result);
        }

        #endregion IGetChildItems
    }
}