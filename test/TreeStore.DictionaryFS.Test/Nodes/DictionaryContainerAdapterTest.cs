using Moq;
using PowerShellFilesystemProviderBase;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
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

        #region IClearItemProperty

        [Fact]
        public void ClearItemProperty_nullifys_dictionary_value()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data1"] = "text",
                ["data2"] = 1
            };
            var rootNode = this.ArrangeContainerAdapter(root);

            // ACT
            rootNode.ClearItemProperty(new[] { "data1", "data2" });

            // ASSERT

            Assert.True(root.TryGetValue("data1", out var v1));
            Assert.Null(v1);
            Assert.True(root.TryGetValue("data2", out var v2));
            Assert.Null(v2);
        }

        [Fact]
        public void ClearItemProperty_ignores_unknown_property()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data"] = "text"
            };
            var rootNode = this.ArrangeContainerAdapter(root);

            // ACT
            rootNode.ClearItemProperty(new[] { "unkown" });

            // ASSERT

            Assert.False(root.TryGetValue("unknown", out var _));
            Assert.True(root.TryGetValue("data", out var value));
            Assert.Equal("text", value);
        }

        [Fact]
        public void ClearItemProperty_ignores_child_node()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data"] = new UnderlyingDictionary()
            };
            var rootNode = this.ArrangeContainerAdapter(root);

            // ACT
            rootNode.ClearItemProperty(new[] { "data" });

            // ASSERT

            Assert.True(root.TryGetValue("data", out var value));
            Assert.IsType<UnderlyingDictionary>(value);
        }

        #endregion IClearItemProperty

        #region ISetItemProperty

        [Fact]
        public void SetItemProperty_sets_dictionary_value()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data1"] = "text",
                ["data2"] = 1
            };
            var rootNode = this.ArrangeContainerAdapter(root);

            // ACT
            rootNode.SetItemProperty(new PSObject(new
            {
                data1 = "changed",
                data2 = 3
            }));

            // ASSERT
            Assert.True(root.TryGetValue("data1", out var v1));
            Assert.Equal("changed", v1);
            Assert.True(root.TryGetValue("data2", out var v2));
            Assert.Equal(3, v2);
        }

        [Fact]
        public void SetItemProperty_ignores_child_nodes()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data1"] = "text",
                ["data2"] = new UnderlyingDictionary
                {
                    ["data3"] = "data3"
                }
            };
            var rootNode = this.ArrangeContainerAdapter(root);

            // ACT
            rootNode.SetItemProperty(new PSObject(new
            {
                data1 = "changed",
                data2 = 3
            }));

            // ASSERT
            Assert.True(root.TryGetValue("data1", out var v1));
            Assert.Equal("changed", v1);
            Assert.True(root.TryGetValue("data2", out var v2));
            Assert.IsType<UnderlyingDictionary>(v2);
        }

        [Fact]
        public void SetItemProperty_gnores_unknown_property()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data1"] = "text",
            };
            var rootNode = this.ArrangeContainerAdapter(root);

            // ACT
            rootNode.SetItemProperty(new PSObject(new
            {
                unkown = "changed",
            }));

            // ASSERT
            Assert.True(root.TryGetValue("data1", out var v1));
            Assert.Equal("text", v1);
            Assert.False(root.TryGetValue("unknown", out var _));
        }

        #endregion ISetItemProperty

        #region ICopyItemProperty

        [Fact]
        public void CopyItemProperty_set_new_properties_value()
        {
            // ARRANGE
            var child = new UnderlyingDictionary
            {
            };
            var childAdapter = this.ArrangeContainerAdapter(child);
            var childNode = new LeafNode("child1", childAdapter);

            var root = new UnderlyingDictionary
            {
                ["data1"] = "text",
                ["child"] = child
            };
            var rootNode = new RootNode(this.ArrangeContainerAdapter(root));

            // ACT
            childNode.CopyItemProperty(rootNode, "data1", "data1");

            // ASSERT
            Assert.True(child.TryGetValue("data1", out var value));
            Assert.Equal("text", value);
        }

        #endregion ICopyItemProperty

        #region IRemoveItemProperty

        [Fact]
        public void RemoveItemProperty_removes_data_property()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data1"] = "text",
            };
            var rootAdapter = this.ArrangeContainerAdapter(root);

            // ACT
            rootAdapter.RemoveItemProperty("data1");

            // ASSERT
            Assert.False(root.TryGetValue("data1", out var value));
        }

        #endregion IRemoveItemProperty

        #region IMoveItemProperty

        [Fact]
        public void MoveItemProperty_moves_property_value()
        {
            // ARRANGE
            var child = new UnderlyingDictionary
            {
            };
            var childAdapter = this.ArrangeContainerAdapter(child);
            var childNode = new LeafNode("child1", childAdapter);

            var root = new UnderlyingDictionary
            {
                ["data1"] = "text",
                ["child"] = child
            };
            var rootNode = new RootNode(this.ArrangeContainerAdapter(root));

            // ACT
            childNode.MoveItemProperty(rootNode, "data1", "data1");

            // ASSERT
            Assert.False(root.TryGetValue("data1", out var _));
            Assert.True(child.TryGetValue("data1", out var value));
            Assert.Equal("text", value);
        }

        #endregion IMoveItemProperty

        #region INewItemProperty

        [Fact]
        public void NewItemProperty_creates_data_property()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
            };
            var rootAdapter = this.ArrangeContainerAdapter(root);

            // ACT
            rootAdapter.NewItemProperty("data1", null, 1);

            // ASSERT
            Assert.True(root.TryGetValue("data1", out var value));
            Assert.Equal(1, value);
        }

        #endregion INewItemProperty

        #region IRenameItemProperty

        [Fact]
        public void RenameItemProperty_renames_data_property()
        {
            // ARRANGE
            var data = "text";
            var root = new UnderlyingDictionary
            {
                ["data"] = data,
            };

            var rootAdapter = this.ArrangeContainerAdapter(root);

            // ACT
            rootAdapter.RenameItemProperty("data", "newname");

            // ASSERT
            Assert.True(root.TryGetValue("newname", out var value));
            Assert.Same(data, value);
        }

        #endregion IRenameItemProperty
    }
}