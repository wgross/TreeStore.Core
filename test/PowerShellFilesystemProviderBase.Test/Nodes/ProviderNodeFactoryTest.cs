using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.Nodes
{
    [Collection(nameof(PowerShell))]
    public class ProviderNodeFactoryTest
    {
        [Fact]
        public void Create_leaf_node_from_object_with_name_property()
        {
            // ACT
            var result = ProviderNodeFactory.Create(new { Name = "name" });

            // ASSERT
            Assert.Equal("name", result.Name);
            Assert.IsType<LeafNode>(result);
        }

        [Fact]
        public void Creating_leaf_node_throws_on_missing_name_property()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => ProviderNodeFactory.Create(new { }));

            // ASSERT
            Assert.Equal("name", result.ParamName);
        }

        [Fact]
        public void Create_container_node_from_Dictionary_with_name_item()
        {
            // ACT
            var result = ProviderNodeFactory.Create(new Dictionary<string, object>() { { "Name", "name" } });

            // ASSERT
            Assert.Equal("name", result.Name);
            Assert.IsType<ContainerNode>(result);
        }

        [Fact]
        public void Creating_container_node_throws_on_missing_name_property()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => ProviderNodeFactory.Create(new Dictionary<string, object>()));

            // ASSERT
            Assert.Equal("name", result.ParamName);
        }

        [Fact]
        public void Creating_container_node_throws_on_missing_name_property_value()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => ProviderNodeFactory.Create(new Dictionary<string, object>() { { "Name", null } }));

            // ASSERT
            Assert.Equal("name", result.ParamName);
        }

        private class Container : IItemContainer
        {
            public string Name { get; set; } = "name";

            public (bool exists, ProviderNode node) TryGetChildNode(string name)

            {
                throw new NotImplementedException();
            }
        }

        [Fact]
        public void Create_container_node_for_IGetChildItem_with_name_property()
        {
            // ACT
            var result = ProviderNodeFactory.Create(new Container());

            // ASSERT
            Assert.IsType<ContainerNode>(result);
            Assert.Equal("name", result.Name);
        }

        [Fact]
        public void Creating_container_node_for_IGetChildItem_with_name_property_thows_on_null_name()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => ProviderNodeFactory.Create(new Container() { Name = null }));

            // ASSERT
            Assert.Equal("name", result.ParamName);
        }
    }
}