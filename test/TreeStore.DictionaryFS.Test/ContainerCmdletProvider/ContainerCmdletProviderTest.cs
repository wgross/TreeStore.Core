using Moq;
using PowerShellFilesystemProviderBase;
using PowerShellFilesystemProviderBase.Capabilities;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace TreeStore.DictionaryFS.Test.ContainerCmdletProvider
{
    [Collection(nameof(PowerShell))]
    public sealed class ContainerCmdletProviderTest : ContainerCmdletProviderTestBase
    {
        #region Get-ChildItem -Path -Recurse

        [Fact]
        public void Powershell_retrieves_roots_childnodes()
        {
            // ARRANGE
            var root = new Dictionary<string, object>
            {
                ["child1"] = new Dictionary<string, object> { },
                ["property"] = "text"
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Single(result);

            var psobject = result.ElementAt(0);

            Assert.Equal("child1", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\child1", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_retrieves_roots_childnodes_recursive()
        {
            // ARRANGE
            var root = new Dictionary<string, object?>
            {
                ["child1"] = new Dictionary<string, object?>
                {
                    ["grandchild"] = new Dictionary<string, object?>()
                    {
                        ["property"] = "text"
                    }
                }
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\")
                .AddParameter("Recurse")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(2, result.Count());

            var psobject = result.ElementAt(0);

            Assert.Equal("child1", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\child1", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSParentPath"));

            psobject = result.ElementAt(1);

            Assert.Equal("grandchild", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\child1\grandchild", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\child1", psobject.Property<string>("PSParentPath"));
        }

        [Fact]
        public void Powershell_retrieves_roots_childnodes_recursive_upto_depth()
        {
            // ARRANGE
            var root = new Dictionary<string, object?>
            {
                ["child1"] = new Dictionary<string, object?>
                {
                    ["grandchild"] = new Dictionary<string, object?>()
                    {
                        ["grandgrandchild"] = new Dictionary<string, object?>()
                    }
                },
                ["property"] = "text",
                ["child2"] = Mock.Of<IItemContainer>()
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\")
                .AddParameter("Recurse")
                .AddParameter("Depth", 1) // only children, no grandchildren
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(2, result.Count());

            var psobject = result.ElementAt(0);

            Assert.Equal("child1", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\child1", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSParentPath"));

            psobject = result.ElementAt(1);

            Assert.Equal("grandchild", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\child1\grandchild", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\child1", psobject.Property<string>("PSParentPath"));
        }

        #endregion Get-ChildItem -Path -Recurse

        #region Remove-Item -Path -Recurse

        [Fact]
        public void Powershell_removes_root_child_node()
        {
            // ARRANGE
            var root = new Dictionary<string, object?>
            {
                ["child1"] = new Dictionary<string, object?>(),
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Remove-Item")
                .AddParameter("Path", @"test:\child1")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(root);
        }

        [Fact]
        public void Powershell_removes_root_child_node_recursive()
        {
            // ARRANGE
            var root = new Dictionary<string, object?>
            {
                ["child1"] = new Dictionary<string, object>
                {
                    ["grandchild1"] = new Dictionary<string, object?>()
                }
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Remove-Item")
                .AddParameter("Path", @"test:\child1")
                .AddParameter("Recurse", true)
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Empty(root);
        }

        #endregion Remove-Item -Path -Recurse

        #region New-Item -Path -ItemType -Value

        [Fact]
        public void Powershell_creates_child_item()
        {
            // ARRANGE
            var root = new Dictionary<string, object?>();
            var child = new Dictionary<string, object?>();

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("New-Item")
                .AddParameter("Path", @"test:\child1")
                .AddParameter("Value", child)
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psobject = result.Single();

            Assert.Equal("child1", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\child1", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSParentPath"));

            Assert.True(root.TryGetValue("child1", out var added));
            Assert.Same(child, added);
        }

        #endregion New-Item -Path -ItemType -Value

        #region Rename-Item -Path -NewName

        [Fact]
        public void Powershell_renames_childitem()
        {
            // ARRANGE
            var child = new Dictionary<string, object?>();
            var root = new Dictionary<string, object?>
            {
                ["child1"] = child
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Rename-Item")
                .AddParameter("Path", @"test:\child1")
                .AddParameter("NewName", "newName")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True(root.TryGetValue("newName", out var renamed));
            Assert.Same(child, renamed);
        }

        #endregion Rename-Item -Path -NewName

        #region Copy-Item -Path -Destination

        [Fact(Skip = "Hard to test without a real implementation")]
        public void Powershell_copy_childitem_invoke_underlying()
        {
            // ARRANGE
            var child1 = new Dictionary<string, object>()
            {
                { "Name", "child1" }
            };

            var destination = this.Mocks.Create<ICopyChildItem>();
            destination
                .Setup(ci => ci.NewChildItemAsCopy("newname", child1));
            destination
                .Setup(ci => ci.CopyChildItemParameters("child1", @"test:\destination\newname", false))
                .Returns(null);

            var root = new Dictionary<string, object>
            {
                { "child1", child1 },
                { "child2", destination }
            };

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Copy-Item")
                .AddParameter("Path", @"test:\child1")
                .AddParameter("Destination", @"test:\destination\newname")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True(child1.TryGetValue("newname", out var dest));
            Assert.True(child1.TryGetValue("child1", out var src));
            Assert.NotSame(child1, src);
            Assert.NotSame(src, dest);
        }

        #endregion Copy-Item -Path -Destination
    }
}