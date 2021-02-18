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
        public void Create_container_node_from_Dictionary_with_name()
        {
            // ACT
            var result = ProviderNodeFactory.Create("name", new Dictionary<string, object>());

            // ASSERT
            Assert.Equal("name", result.Name);
            Assert.IsType<ContainerNode>(result);
        }

        [Fact]
        public void Creating_container_node_throws_on_missing_name()
        {
            // ACT
            var result = Assert.Throws<ArgumentNullException>(() => ProviderNodeFactory.Create(null, new Dictionary<string, object>()));

            // ASSERT
            Assert.Equal("name", result.ParamName);
        }
    }
}