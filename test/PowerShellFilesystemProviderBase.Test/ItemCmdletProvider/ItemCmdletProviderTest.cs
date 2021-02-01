using PowerShellFilesystemProviderBase.Capabilities;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.ItemCmdletProvider
{
    public class ItemCmdletProviderTest : ItemCmdletProviderTestBase
    {
        #region Get-Item -Path

        [Fact]
        public void Powershell_retrieves_root_leaf()
        {
            // ARRANGE
            var root = new { };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Get-Item")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psobject = result.Single();

            Assert.Equal(string.Empty, psobject.Property<string>("PSChildName"));
            Assert.False(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_retrieves_toplevel_container_item_by_name()
        {
            // ARRANGE
            var root = new Dictionary<string, object>
            {
                { "item" , new Dictionary<string, object>() }
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

        [Fact]
        public void Powershell_retrieves_toplevel_leaf_item_by_name()
        {
            // ARRANGE
            var root = new Dictionary<string, object>
            {
                { "item", new { } }
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
            Assert.False(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\item", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_reads_item_from_provider_node()
        {
            // ARRANGE

            var pso = new PSObject();
            var getItemMock = this.Mocks.Create<IGetItem>();
            getItemMock
                .Setup(gi => gi.GetItem())
                .Returns(pso);
            getItemMock
                .Setup(gi => gi.GetItemParameters())
                .Returns(new { });

            var root = new Dictionary<string, object>
            {
                { "item", getItemMock.Object }
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
            Assert.False(psobject.Property<bool>("PSIsContainer"));
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
            var pso = new PSObject();
            var setItemMock = this.Mocks.Create<ISetItem>();
            setItemMock
                .Setup(gi => gi.SetItem(1));
            setItemMock
                .Setup(gi => gi.SetItemParameters())
                .Returns(new { });

            this.ArrangeFileSystem(setItemMock.Object);

            // ACT
            var result = this.PowerShell.AddCommand("Set-Item")
                .AddParameter("Path", @"test:\")
                .AddParameter("Value", 1)
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Set-Item -Path

        #region Clear-Item -Path

        [Fact]
        public void Powershell_clears_item_value()
        {
            // ARRANGE
            var pso = new PSObject();
            var setItemMock = this.Mocks.Create<IClearItem>();
            setItemMock
                .Setup(ci => ci.ClearItem());
            setItemMock
                .Setup(ci => ci.ClearItemParameters())
                .Returns(new { });

            this.ArrangeFileSystem(setItemMock.Object);

            // ACT
            var result = this.PowerShell.AddCommand("Clear-Item")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Clear-Item -Path

        #region Test-Path -Path -PathType

        [Fact]
        public void Powershell_test_root_path()
        {
            // ARRANGE
            var pso = new PSObject();
            var itemExistsMock = this.Mocks.Create<IItemExists>();
            itemExistsMock
                .Setup(ie => ie.ItemExists())
                .Returns(true);
            itemExistsMock
                .Setup(ie => ie.ItemExistsParameters())
                .Returns(new { });

            this.ArrangeFileSystem(itemExistsMock.Object);

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
            var pso = new PSObject();
            var itemExistsMock = this.Mocks.Create<IItemExists>();
            itemExistsMock
                .Setup(ie => ie.ItemExists())
                .Returns(true);
            itemExistsMock
                .Setup(ie => ie.ItemExistsParameters())
                .Returns(new { });

            this.ArrangeFileSystem(itemExistsMock.Object);

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
        public void Powershell_tests_toplevel_path_as_container()
        {
            // ARRANGE
            var root = new Dictionary<string, object>
            {
                { "item" , new Dictionary<string, object>() }
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Test-Path")
                .AddParameter("Path", @"test:\item")
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
        public void Powershell_invokes_default_action()
        {
            // ARRANGE
            var itemExistsMock = this.Mocks.Create<IInvokeItem>();
            itemExistsMock
                .Setup(ie => ie.InvokeItem());
            itemExistsMock
                .Setup(ie => ie.InvokeItemParameters())
                .Returns(new { });

            // ACT
            var result = this.PowerShell.AddCommand("Invoke-Item")
                .AddParameter("Path", @"test:\item")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(result);
        }

        #endregion Invoke-Item -Path
    }
}