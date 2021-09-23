using PowerShellFilesystemProviderBase;
using System.Linq;
using System.Management.Automation;
using Xunit;
using UnderlyingDictionary = System.Collections.Generic.Dictionary<string, object?>;

namespace TreeStore.DictionaryFS.Test.PropertyCmdletProvider
{
    [Collection(nameof(PowerShell))]
    public class PropertyCmdletProviderTest : DynamicPropertyCmdletProviderTestBase
    {
        [Fact]
        public void Powershell_creates_item_property()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("New-ItemProperty")
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
            Assert.Equal("DictionaryFS", result.Single().Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TreeStore.DictionaryFS\DictionaryFS::test:\", result.Single().Property<string>("PSPath"));
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

        [Fact]
        public void Powershell_removes_item_property()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data"] = 1
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Remove-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "data")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.False(root.TryGetValue("data", out var _));
        }

        [Fact]
        public void Powershell_renames_item_property()
        {
            // ARRANGE
            var root = new UnderlyingDictionary
            {
                ["data"] = 1
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Rename-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "data")
                .AddParameter("NewName", "data-changed")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True(root.TryGetValue("data-changed", out var value));
            Assert.Equal(1, value);
        }

        [Fact]
        public void Powershell_copies_item_property()
        {
            // ARRANGE
            var child = new UnderlyingDictionary
            {
            };

            var root = new UnderlyingDictionary
            {
                ["child"] = child,
                ["data"] = 1
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Copy-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "data")
                .AddParameter("Destination", @"test:\child")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True(root.TryGetValue("data", out var value1));
            Assert.Equal(1, value1);
            Assert.True(child.TryGetValue("data", out var value2));
            Assert.Equal(1, value2);
        }

        [Fact]
        public void Powershell_moves_item_property()
        {
            // ARRANGE
            var child = new UnderlyingDictionary
            {
            };

            var root = new UnderlyingDictionary
            {
                ["child"] = child,
                ["data"] = 1
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Move-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "data")
                .AddParameter("Destination", @"test:\child")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.False(root.TryGetValue("data", out var _));
            Assert.True(child.TryGetValue("data", out var value));
            Assert.Equal(1, value);
        }
    }
}