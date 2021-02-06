using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    public class DictionaryContainerAdapterTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose() => this.mocks.VerifyAll();

        private DictionaryContainerNode<IDictionary<string, object>, object> ArrangeContainerNode(string name, IDictionary<string, object> dictionary)
        {
            return new DictionaryContainerNode<IDictionary<string, object>, object>(dictionary);
        }

        #region IContainerItem

        [Fact]
        public void TryGetChildItem_gets_child_dictionary_container_by_name()
        {
            // ARRANGE
            var node = this.ArrangeContainerNode("name", new Dictionary<string, object>
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
        public void TryGetchildItem_ignores_data_property()
        {
            // ARRANGE
            var node = this.ArrangeContainerNode("name", new Dictionary<string, object>
            {
                { "data", new { } }
            }); ;

            // ACT
            var result = node.TryGetChildNode("data");

            // ASSERT
            Assert.False(result.exists);
        }

        #endregion IContainerItem

        #region IGetItem

        [Fact]
        public void GetItem_creates_PSObject_of_Dictionary()
        {
            // ARRANGE
            var data = new object();
            var child = new Dictionary<string, object>();
            var node = ContainerNodeFactory.Create("name", new Dictionary<string, object>
            {
                { "data", data },
                { "null", (string)null },
                { "child", child }
            });

            // ACT
            var result = node.GetItem();

            // ASSERT
            Assert.Equal("name", result.Property<string>("PSChildName"));
            Assert.Same(data, result.Property<object>("data"));
            Assert.Null(result.Property<object>("null"));
            Assert.Same(child, result.Property<object>("child"));
        }

        #endregion IGetItem

        #region IGetChildItem

        [Fact]
        public void GetChildItems_gets_containers()
        {
            // ARRANGE
            var underlying = new Dictionary<string, object>
            {
                { "container1", new Dictionary<string, object> { { "leaf", new { } } } },
                { "property" , "text" },
                { "container2", Mock.Of<IItemContainer>() },
            };

            var node = this.ArrangeContainerNode("name", underlying);

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Equal(2, result.Count());
            Assert.Equal(new[] { "container1", "container2" }, result.Select(n => n.Name));
            Assert.All(result, r => Assert.True(r.IsContainer));
        }

        [Fact]
        public void GetChildItems_from_empty_returns_empty()
        {
            // ARRANGE
            var underlying = new Dictionary<string, object>();
            var node = this.ArrangeContainerNode("name", underlying);

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Empty(result);
        }

        [Fact]
        public void HasChildItem_returns_GetChildItems_Any()
        {
            // ARRANGE
            var node = this.ArrangeContainerNode("name", new Dictionary<string, object>
            {
                { "child", new Dictionary<string,object>()}
            });

            // ACT
            var result = node.HasChildItems();

            // ASSERT
            Assert.True(result);
        }

        #endregion IGetChildItem

        #region IRemoveChildItem

        [Fact]
        public void RemoveChildItem_removes_dictionary_item()
        {
            // ARRANGE
            var underlying = new Dictionary<string, object>
            {
                { "container1", new Dictionary<string, object> { { "leaf", new { } } } },
                { "property" , "text" },
                { "container2", Mock.Of<IItemContainer>() },
            };

            var node = this.ArrangeContainerNode("name", underlying);

            // ACT
            node.RemoveChildItem("container1");

            // ASSERT
            Assert.False(underlying.TryGetValue("container1", out var _));
        }

        #endregion IRemoveChildItem

        #region INewChildItem

        [Fact]
        public void NewChildItem_creates_dictionary_item()
        {
            // ARRANGE
            var underlying = new Dictionary<string, object>
            {
                { "property" , "text" },
                { "container2", Mock.Of<IItemContainer>() },
            };

            var node = this.ArrangeContainerNode("name", underlying);

            // ACT
            var value = new Dictionary<string, object>();
            var result = node.NewChildItem("container1", "itemTypeValue", value);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("container1", result.Name);
            Assert.True(result.IsContainer);
            Assert.True(underlying.TryGetValue("container1", out var added));
            Assert.Same(value, added);
        }

        #endregion INewChildItem
    }
}