using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    public class ContainerNodeFactoryTest
    {
        [Fact]
        public void Create_container_node_from_Dictionary()
        {
            // ACT
            var result = ContainerNodeFactory.Create("name", new Dictionary<string, object>());

            // ASSERT
            Assert.True(result.IsContainer);
            Assert.Equal("name", result.Name);
            Assert.IsType<ContainerNode>(result);
        }

        [Fact]
        public void Create_container_node_from_IDictionary()
        {
            // ACT
            var result = ContainerNodeFactory.Create("name", Mock.Of<IDictionary<string, object>>());

            // ASSERT
            Assert.True(result.IsContainer);
            Assert.Equal("name", result.Name);
            Assert.IsType<ContainerNode>(result);
        }

        [Fact]
        public void Create_container_node_from_IGetChildItem()
        {
            // ACT
            var result = ContainerNodeFactory.Create("name", Mock.Of<IItemContainer>());

            // ASSERT
            Assert.IsType<ContainerNode>(result);
        }

        [Fact]
        public void Creating_container_node_throws_on_null_name()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => ContainerNodeFactory.Create(null, new Dictionary<string, object>()));

            // ASSERT
            Assert.Equal("name", result.ParamName);
        }

        [Fact]
        public void Creating_container_node_throws_on_null_underlying()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => ContainerNodeFactory.Create("name", null));

            // ASSERT
            Assert.Equal("underlying", result.ParamName);
        }
    }
}