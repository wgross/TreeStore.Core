using Moq;
using PowerShellFilesystemProviderBase;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using TreeStore.DictionaryFS.Nodes;
using Xunit;
using IUnderlyingDictionary = System.Collections.Generic.IDictionary<string, object?>;
using UnderlyingDictionary = System.Collections.Generic.Dictionary<string, object?>;

namespace TreeStore.DictionaryFS.Test.Nodes
{
    public class DictionaryContainerAdapterTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose() => this.mocks.VerifyAll();

        private DictionaryContainerAdapter ArrangeContainerAdapter(IUnderlyingDictionary dictionary)
        {
            return new DictionaryContainerAdapter(dictionary);
        }

        #region IContainerItem

        [Fact]
        public void TryGetChildItem_gets_child_dictionary_container_by_name()
        {
            // ARRANGE
            var node = this.ArrangeContainerAdapter(new UnderlyingDictionary
            {
                { "container", new UnderlyingDictionary { } }
            });

            // ACT
            var result = node.TryGetChildNode("container");

            // ASSERT
            Assert.True(result.exists);
            Assert.Equal("container", result.node.Name);
            Assert.True(result.node is ContainerNode);
        }

        [Fact]
        public void TryGetChildItem_ignores_data_property()
        {
            // ARRANGE
            var node = this.ArrangeContainerAdapter(new UnderlyingDictionary
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
            var child = new UnderlyingDictionary();
            var node = this.ArrangeContainerAdapter(new UnderlyingDictionary
            {
                ["data"] = data,
                ["null"] = null,
                ["child"] = child
            });

            // ACT
            var result = node.GetItem();

            // ASSERT
            Assert.Same(data, result!.Property<object>("data"));
            Assert.Null(result!.Property<object>("null"));
            Assert.Same(child!, result!.Property<object>("child"));
        }

        #endregion IGetItem

        #region ISetItem

        [Fact]
        public void SetItem_replaces_dictionary_content()
        {
            // ARRANGE

            var node = this.ArrangeContainerAdapter(new UnderlyingDictionary
            {
                ["child"] = new UnderlyingDictionary()
            });
            var newData = new UnderlyingDictionary()
            {
                ["data"] = new { }
            };

            // ACT
            node.SetItem(newData);

            // ASSERT
            Assert.Equal(newData.Values, node.Underlying.Values);
            Assert.Equal(newData.Keys, node.Underlying.Keys);
        }

        [Fact]
        public void SetItem_rejects_unkown_type()
        {
            // ARRANGE

            var node = this.ArrangeContainerAdapter(new UnderlyingDictionary
            {
                ["child"] = new UnderlyingDictionary()
            });

            // ACT
            var result = Assert.Throws<InvalidOperationException>(() => node.SetItem(new object()));

            // ASSERT
            Assert.Equal("Data of type 'System.Object' can't be assigned", result.Message);
        }

        [Fact]
        public void SetItem_rejects_null()
        {
            // ARRANGE
            var node = this.ArrangeContainerAdapter(new UnderlyingDictionary
            {
                ["child"] = new UnderlyingDictionary()
            });

            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => node.SetItem(null));

            // ASSERT
            Assert.Equal("value", result.ParamName);
        }

        #endregion ISetItem

        #region IClearItem

        [Fact]
        public void ClearItem_clear_dictionary()
        {
            // ARRANGE
            var node = this.ArrangeContainerAdapter(new UnderlyingDictionary
            {
                ["child"] = new UnderlyingDictionary()
            });

            // ACT
            node.ClearItem();

            // ASSERT
            Assert.Empty(node.Underlying);
        }

        #endregion IClearItem

        #region IGetChildItem

        [Fact]
        public void GetChildItems_gets_containers()
        {
            // ARRANGE
            var underlying = new UnderlyingDictionary
            {
                ["container1"] = new UnderlyingDictionary
                {
                    ["leaf"] = new { }
                },
                ["property"] = "text"
            };

            var node = this.ArrangeContainerAdapter(underlying);

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Single(result);
            Assert.Equal(new[] { "container1" }, result.Select(n => n.Name));
            Assert.All(result, r => Assert.True(r is ContainerNode));
        }

        [Fact]
        public void GetChildItems_from_empty_returns_empty()
        {
            // ARRANGE
            var underlying = new UnderlyingDictionary();
            var node = this.ArrangeContainerAdapter(underlying);

            // ACT
            var result = node.GetChildItems().ToArray();

            // ASSERT
            Assert.Empty(result);
        }

        [Fact]
        public void HasChildItem_returns_GetChildItems_Any()
        {
            // ARRANGE
            var node = this.ArrangeContainerAdapter(new UnderlyingDictionary
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
            var underlying = new UnderlyingDictionary
            {
                { "container1", new UnderlyingDictionary { { "leaf", new { } } } },
                { "property" , "text" },
                { "container2", Mock.Of<IItemContainer>() },
            };

            var node = this.ArrangeContainerAdapter(underlying);

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
            var underlying = new UnderlyingDictionary
            {
                { "property" , "text" },
                { "container2", Mock.Of<IItemContainer>() },
            };

            var node = this.ArrangeContainerAdapter(underlying);

            // ACT
            var value = new UnderlyingDictionary()
            {
                { "Name", "container1" }
            };
            var result = node.NewChildItem("container1", "itemTypeValue", value);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("container1", result.Name);
            Assert.True(result is ContainerNode);
            Assert.True(underlying.TryGetValue("container1", out var added));
            Assert.Same(value, added);
        }

        [Fact]
        public void NewChildItem_creates_dictionary_item_other_Dictionary_container()
        {
            // ARRANGE
            var underlying = new UnderlyingDictionary
            {
                { "property" , "text" },
                { "container2", Mock.Of<IItemContainer>() },
            };

            var node = this.ArrangeContainerAdapter(underlying);

            // ACT
            var value = new UnderlyingDictionary();
            var result = node.NewChildItem("container1", "itemTypeValue", value);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal("container1", result.Name);
            Assert.True(result is ContainerNode);
            Assert.True(underlying.TryGetValue("container1", out var added));
            Assert.Same(value, added);
        }

        #endregion INewChildItem

        #region IRenameChildItem

        [Fact]
        public void RenameChildItem_renames_property()
        {
            var underlying = new UnderlyingDictionary
            {
                ["container1"] = Mock.Of<IItemContainer>(),
            };

            var node = this.ArrangeContainerAdapter(underlying);

            // ACT
            node.RenameChildItem("container1", "newname");

            // ASSERT
            Assert.True(underlying.TryGetValue("newname", out var _));
            Assert.False(underlying.TryGetValue("container1", out var _));
        }

        #endregion IRenameChildItem

        #region IMoveChildItem

        [Fact]
        public void MoveChildItem_moves_underlying()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["child1"] = new UnderlyingDictionary(),
                ["child2"] = new UnderlyingDictionary()
            };
            var nodetoMove = root.AsDictionary("child1");
            var rootNode = new RootNode(this.ArrangeContainerAdapter(root));
            var dst = this.ArrangeContainerAdapter(root.AsDictionary("child2"));

            // ACT
            // move child1 under child2 as child1
            dst.MoveChildItem(rootNode, rootNode.GetChildItems().Single(n => n.Name == "child1"), destination: new string[0]);

            // ASSERT
            Assert.Same(nodetoMove, root.AsDictionary("child2").AsDictionary("child1"));
            Assert.False(root.TryGetValue("child1", out var _));
        }

        [Fact]
        public void MoveChildItem_moves_underlying_with_new_name()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["child1"] = new UnderlyingDictionary(),
                ["child2"] = new UnderlyingDictionary()
            };
            var nodetoMove = root.AsDictionary("child1");
            var rootNode = new RootNode(this.ArrangeContainerAdapter(root));
            var dst = this.ArrangeContainerAdapter(root.AsDictionary("child2"));

            // ACT
            // move child1 under child2 as newname
            dst.MoveChildItem(rootNode, rootNode.GetChildItems().Single(n => n.Name == "child1"), destination: new string[] { "newname" });

            // ASSERT
            Assert.Same(nodetoMove, root.AsDictionary("child2").AsDictionary("newname"));
            Assert.False(root.TryGetValue("child1", out var _));
        }

        [Fact]
        public void MoveChildItem_moves_underlying_with_new_parent_and_name()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["child1"] = new UnderlyingDictionary(),
                ["child2"] = new UnderlyingDictionary()
            };
            var nodetoMove = root.AsDictionary("child1");
            var rootNode = new RootNode(this.ArrangeContainerAdapter(root));
            var dst = this.ArrangeContainerAdapter(root.AsDictionary("child2"));

            // ACT
            // move child1 under child2 as newparent/newname
            dst.MoveChildItem(rootNode, rootNode.GetChildItems().Single(n => n.Name == "child1"), destination: new string[] { "newparent", "newname" });

            // ASSERT
            Assert.Same(nodetoMove, root.AsDictionary("child2").AsDictionary("newparent").AsDictionary("newname"));
            Assert.False(root.TryGetValue("child1", out var _));
        }

        #endregion IMoveChildItem

        #region ICopyChildItem

        [Fact]
        public void CopyChildItem_copies_underlying_shallow()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["child1"] = new UnderlyingDictionary
                {
                    ["data"] = 1,
                    ["grandchild"] = new UnderlyingDictionary()
                },
                ["child2"] = new UnderlyingDictionary()
            };
            var nodeToCopy = root.AsDictionary("child1");
            var rootNode = new RootNode(this.ArrangeContainerAdapter(root));
            var dst = this.ArrangeContainerAdapter(root.AsDictionary("child2"));

            // ACT
            // copy child1 under child2 as 'child1'
            var result = dst.CopyChildItem(rootNode.GetChildItems().Single(n => n.Name == "child1"), destination: new string[0]);

            // ASSERT
            Assert.IsType<ContainerNode>(result);
            Assert.NotNull(root.AsDictionary("child2").AsDictionary("child1"));
            Assert.NotSame(nodeToCopy, root.AsDictionary("child2").AsDictionary("child1"));
            Assert.Same(root.AsDictionary("child2").AsDictionary("child1"), ((DictionaryContainerAdapter)result!.Underlying).Underlying);
            Assert.Same(nodeToCopy, root.AsDictionary("child1"));
            Assert.Equal(1, root.AsDictionary("child2").AsDictionary("child1")["data"]);
            Assert.False(root.AsDictionary("child2").AsDictionary("child1").TryGetValue("grandchild", out var _));
        }

        [Fact]
        public void CopyChildItem_copies_underlying_shallow_with_new_name()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["child1"] = new UnderlyingDictionary
                {
                    ["data"] = 1,
                    ["grandchild"] = new UnderlyingDictionary()
                },
                ["child2"] = new UnderlyingDictionary()
            };
            var nodeToCopy = root.AsDictionary("child1");
            var rootNode = new RootNode(this.ArrangeContainerAdapter(root));
            var dst = this.ArrangeContainerAdapter(root.AsDictionary("child2"));

            // ACT
            // copy child1 under child2 as 'newname'
            var result = dst.CopyChildItem(rootNode.GetChildItems().Single(n => n.Name == "child1"), destination: new string[] { "newname" });

            // ASSERT
            Assert.NotNull(root.AsDictionary("child2").AsDictionary("newname"));
            Assert.NotSame(nodeToCopy, root.AsDictionary("child2").AsDictionary("newname"));
            Assert.Same(nodeToCopy, root.AsDictionary("child1"));
            Assert.Equal(1, root.AsDictionary("child2").AsDictionary("newname")["data"]);
            Assert.False(root.AsDictionary("child2").AsDictionary("newname").TryGetValue("grandchild", out var _));
        }

        [Fact]
        public void CopyChildItem_copies_underlying_shallow_with_new_parent_and_name()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["child1"] = new UnderlyingDictionary
                {
                    ["data"] = 1,
                    ["grandchild"] = new UnderlyingDictionary()
                },
                ["child2"] = new UnderlyingDictionary()
            };
            var nodeToCopy = root.AsDictionary("child1");
            var rootNode = new RootNode(this.ArrangeContainerAdapter(root));
            var dst = this.ArrangeContainerAdapter(root.AsDictionary("child2"));

            // ACT
            // copy child1 under child2 as 'child1'
            var result = dst.CopyChildItem(rootNode.GetChildItems().Single(n => n.Name == "child1"), destination: new string[] { "newparent", "newname" });

            // ASSERT
            Assert.NotNull(root.AsDictionary("child2").AsDictionary("newparent").AsDictionary("newname"));
            Assert.NotSame(nodeToCopy, root.AsDictionary("child2").AsDictionary("newparent").AsDictionary("newname"));
            Assert.NotSame(nodeToCopy, root.AsDictionary("child2").AsDictionary("newparent").AsDictionary("newname"));
            Assert.Same(nodeToCopy, root.AsDictionary("child1"));
            Assert.Equal(1, root.AsDictionary("child2").AsDictionary("newparent").AsDictionary("newname")["data"]);
            Assert.False(root.AsDictionary("child2").AsDictionary("newparent").AsDictionary("newname").TryGetValue("grandchild", out var _));
        }

        #endregion ICopyChildItem
    }
}