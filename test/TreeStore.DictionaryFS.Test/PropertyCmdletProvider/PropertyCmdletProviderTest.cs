using PowerShellFilesystemProviderBase;
using System.Linq;
using System.Management.Automation;
using Xunit;
using UnderlyingDictionary = System.Collections.Generic.Dictionary<string, object?>;

namespace TreeStore.DictionaryFS.Test.PropertyCmdletProvider
{
    public class PropertyCmdletProviderTest : DynamicPropertyCmdletProviderTestBase
    {
        [Fact]
        public void PowerShell_clears_item_property_value()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data"] = 1
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Clear-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "data")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
            Assert.Null(root["data"]);
        }

        [Fact]
        public void Powershell_gets_item_property()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data"] = 1
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Get-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "data")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(1, result.Single().Property<int>("data"));
            Assert.True(result.Single().Property<bool>("PSIsContainer"));
            Assert.Equal("test", result.Single().Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", result.Single().Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", result.Single().Property<string>("PSPath"));
            Assert.Equal(@"", result.Single().Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_sets_item_property()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data"] = 1
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Set-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "data")
                .AddParameter("Value", "text")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True(root.TryGetValue("data", out var value));
            Assert.Equal("text", value);
        }
    }
}