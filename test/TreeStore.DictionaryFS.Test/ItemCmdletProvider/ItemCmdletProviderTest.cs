using PowerShellFilesystemProviderBase;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace TreeStore.DictionaryFS.Test.ItemCmdletProvider
{
    [Collection(nameof(PowerShell))]
    public sealed class ItemCmdletProviderTest : ItemCmdletProviderTestBase
    {
        #region Get-Item -Path

        [Fact]
        public void Powershell_retrieves_root_node()
        {
            // ARRANGE
            this.ArrangeFileSystem(new Dictionary<string, object?>());

            // ACT
            var result = this.PowerShell.AddCommand("Get-Item")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psobject = result.Single();

            Assert.Equal(string.Empty, psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSPath"));
            Assert.Equal(string.Empty, psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_retrieves_child_node()
        {
            // ARRANGE
            var root = new Dictionary<string, object?>
            {
                ["item"] = new Dictionary<string, object>()
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Get-Item")
                .AddParameter("Path", @"test:\item")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psobject = result.Single();

            Assert.Equal("item", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\item", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSParentPath"));
        }

        #endregion Get-Item -Path

        #region Set-Item -Path

        [Fact]
        public void Powershell_sets_item_value()
        {
            // ARRANGE
            var root = new Dictionary<string, object?>
            {
                ["child"] = new Dictionary<string, object?>()
            };

            this.ArrangeFileSystem(root);
            var newValue = new Dictionary<string, object?>
            {
                ["data"] = new object()
            };

            // ACT
            var result = this.PowerShell.AddCommand("Set-Item")
                .AddParameter("Path", @"test:\child")
                .AddParameter("Value", newValue)
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
            Assert.NotSame(newValue, root["child"]);
            Assert.Same(newValue["data"], ((IDictionary<string, object>)root["child"]!)["data"]);
        }

        #endregion Set-Item -Path

        #region Clear-Item -Path

        [Fact]
        public void Powershell_clears_item_value()
        {
            // ARRANGE
            var node = new Dictionary<string, object?>
            {
                ["child"] = new object()
            };

            this.ArrangeFileSystem(node);

            // ACT
            var result = this.PowerShell.AddCommand("Clear-Item")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
            Assert.Empty(node);
        }

        #endregion Clear-Item -Path

        #region Test-Path -Path -PathType

        [Fact]
        public void Powershell_test_root_path()
        {
            // ARRANGE
            this.ArrangeFileSystem(new Dictionary<string, object?>());

            // ACT
            var result = this.PowerShell.AddCommand("Test-Path")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True((bool)result.Single().BaseObject);
        }

        [Fact]
        public void Powershell_test_root_path_as_container()
        {
            // ARRANGE
            this.ArrangeFileSystem(new Dictionary<string, object?>());

            // ACT
            var result = this.PowerShell.AddCommand("Test-Path")
                .AddParameter("Path", @"test:\")
                .AddParameter("PathType", "Container")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True((bool)result.Single().BaseObject);
        }

        [Fact]
        public void Powershell_test_child_path()
        {
            // ARRANGE
            this.ArrangeFileSystem(new Dictionary<string, object?>
            {
                ["child"] = new Dictionary<string, object>()
            });

            // ACT
            var result = this.PowerShell.AddCommand("Test-Path")
                .AddParameter("Path", @"test:\child")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True((bool)result.Single().BaseObject);
        }

        [Fact]
        public void Powershell_test_child_path_as_container()
        {
            // ARRANGE
            this.ArrangeFileSystem(new Dictionary<string, object?>
            {
                ["child"] = new Dictionary<string, object>()
            });

            // ACT
            var result = this.PowerShell.AddCommand("Test-Path")
                .AddParameter("Path", @"test:\child")
                .AddParameter("PathType", "Container")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True((bool)result.Single().BaseObject);
        }

        #endregion Test-Path -Path -PathType

        #region Invoke-Item -Path

        [Fact]
        public void Powershell_invoke_default_action_fails()
        {
            // ARRANGE
            var root = new Dictionary<string, object>
            {
                ["item"] = new object()
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Invoke-Item")
                .AddParameter("Path", @"test:\item")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.True(this.PowerShell.HadErrors);
        }

        #endregion Invoke-Item -Path
    }
}