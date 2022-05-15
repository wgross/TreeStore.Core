using TreeStore.Core.Nodes;
using System;
using Xunit;
using static TreeStore.Core.Test.TestData;

namespace TreeStore.Core.Test.Nodes
{
    public class LeafNodeFactoryTest
    {
        [Fact]
        public void Create_leaf_node_from_object()
        {
            // ACT
            var result = new LeafNode("name", ServiceProvider());

            // ASSERT
            Assert.Equal("name", result.Name);
            Assert.IsType<LeafNode>(result);
        }

        [Fact]
        public void Creating_leaf_node_from_object_throws_on_null_name()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => new LeafNode(null, ServiceProvider()));

            // ASSERT
            Assert.Equal("name", result.ParamName);
        }

        [Fact]
        public void Creating_leaf_node_from_object_throws_on_null_underlying()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => new LeafNode("name", null));

            // ASSERT
            Assert.Equal("underlying", result.ParamName);
        }
    }
}