using Moq;
using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Test.ItemCmdletProvider;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.ContainerCmdletProvider
{
    public class ContainerCmdletProviderTest : ItemCmdletProviderTestBase
    {
        #region Get-Item -Path

        [Fact]
        public void Powershell_retrieves_roots_childnodes()
        {
            // ARRANGE
            var root = new Dictionary<string, object>
            {
                { "container1", new Dictionary<string, object> { { "leaf", new { } } } },
                { "property" , "text" },
                { "container2", Mock.Of<IItemContainer>() },
            }; ;

            this.ArrangeFileSystem(root);

            // ACT
            var result = this.PowerShell.AddCommand("Get-ChildItem")
                .AddParameter("Path", @"test:\")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.Equal(2, result.Count());

            var psobject = result.ElementAt(0);

            Assert.Equal("container1", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\container1", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSParentPath"));

            psobject = result.ElementAt(1);

            Assert.Equal("container2", psobject.Property<string>("PSChildName"));
            Assert.True(psobject.Property<bool>("PSIsContainer"));
            Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
            Assert.Equal("TestFilesystem", psobject.Property<ProviderInfo>("PSProvider").Name);
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\container2", psobject.Property<string>("PSPath"));
            Assert.Equal(@"TestFileSystem\TestFilesystem::test:\", psobject.Property<string>("PSParentPath"));
        }

        #endregion Get-Item -Path
    }
}