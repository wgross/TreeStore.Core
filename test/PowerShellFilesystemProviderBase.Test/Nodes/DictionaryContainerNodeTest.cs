using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    public class DictionaryContainerAdapterTest
    {
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
            var node = ContainerNodeFactory.Create("name", new Dictionary<string, object>
            {
                { "data", data },
                { "null", (string)null }
            });

            // ACT
            var result = node.GetItem();

            // ASSERT
            Assert.Equal("name", result.Property<string>("PSChildName"));
            Assert.Same(data, result.Property<object>("data"));
            Assert.Null(result.Property<object>("null"));
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

        #endregion IGetChildItem
    }
}