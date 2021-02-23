using PowerShellFilesystemProviderBase.Nodes;
using System;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    public class LeafNodeFactoryTest
    {
        [Fact]
        public void Create_leaf_node_from_object()
        {
            // ACT
            var result = LeafNodeFactory.Create("name", new { });

            // ASSERT
            Assert.Equal("name", result.Name);
            Assert.IsType<LeafNode>(result);
        }

        [Fact]
        public void Creating_leaf_node_from_object_throws_on_null_name()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => LeafNodeFactory.Create(null, new { }));

            // ASSERT
            Assert.Equal("name", result.ParamName);
        }

        [Fact]
        public void Creating_leaf_node_from_object_throws_on_null_underlying()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => LeafNodeFactory.Create("name", null));

            // ASSERT
            Assert.Equal("underlying", result.ParamName);
        }
    }
}