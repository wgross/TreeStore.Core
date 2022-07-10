﻿using Moq;
using System;
using System.Management.Automation.Provider;
using TreeStore.Core.Nodes;
using Xunit;
using static TreeStore.Core.Test.TestData;

namespace TreeStore.Core.Test.Nodes
{
    public class LeafNodeFactoryTest
    {
        private readonly MockRepository mocks = new(MockBehavior.Strict);
        private readonly Mock<CmdletProvider> providerMock;

        public LeafNodeFactoryTest()
        {
            this.providerMock = this.mocks.Create<CmdletProvider>();
        }

        [Fact]
        public void Create_leaf_node_from_object()
        {
            // ACT
            var result = new LeafNode(this.providerMock.Object, "name", ServiceProvider());

            // ASSERT
            Assert.Equal("name", result.Name);
            Assert.IsType<LeafNode>(result);
        }

        [Fact]
        public void Creating_leaf_node_from_object_throws_on_null_name()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => new LeafNode(this.providerMock.Object, null, ServiceProvider()));

            // ASSERT
            Assert.Equal("name", result.ParamName);
        }

        [Fact]
        public void Creating_leaf_node_from_object_throws_on_null_underlying()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => new LeafNode(this.providerMock.Object, "name", null));

            // ASSERT
            Assert.Equal("underlying", result.ParamName);
        }
    }
}