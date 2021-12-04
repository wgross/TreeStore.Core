using System.Linq;
using Xunit;
using UnderlyingDictionary = System.Collections.Generic.Dictionary<string, object?>;

namespace TreeStore.DictionaryFS.Test.DynamicPropertyCmdletProvider
{
    [Collection(nameof(PowerShell))]
    public class DynamicPropertyCmdletProviderTest : DynamicPropertyCmdletProviderTestBase
    {
        #region Copy-Item -Path -Destination -Name

        [Fact]
        public void Powershell_copies_item_property()
        {
            // ARRANGE
            var child = new UnderlyingDictionary();
            var root = new UnderlyingDictionary
            {
                ["data"] = 1,
                ["child"] = child
            };

            this.ArrangeFileSystem(root);

            // ACT
            var _ = this.PowerShell.AddCommand("Copy-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Destination", @"test:\child")
                .AddParameter("Name", "data")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True(child.TryGetValue("data", out var value));
            Assert.Equal(1, value);
        }

        #endregion Copy-Item -Path -Destination -Name

        #region Remove-ItemProperty -Path -Name

        [Fact]
        public void Powershell_removes_item_propery()
        {
            // ARRANGE
            var child = new UnderlyingDictionary();
            var root = new UnderlyingDictionary
            {
                ["data"] = 1,
                ["child"] = child
            };

            this.ArrangeFileSystem(root);

            // ACT
            var _ = this.PowerShell.AddCommand("Remove-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "data")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.False(child.TryGetValue("data", out var _));
        }

        #endregion Remove-ItemProperty -Path -Name

        #region Move-ItemProperty -Path -Destination -Name

        [Fact]
        public void Powershell_moves_item_property()
        {
            // ARRANGE
            var child = new UnderlyingDictionary();
            var root = new UnderlyingDictionary
            {
                ["data"] = 1,
                ["child"] = child
            };

            this.ArrangeFileSystem(root);

            // ACT
            var _ = this.PowerShell.AddCommand("Move-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Destination", @"test:\child")
                .AddParameter("Name", "data")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.False(root.TryGetValue("data", out var _));
            Assert.True(child.TryGetValue("data", out var value));
            Assert.Equal(1, value);
        }

        #endregion Move-ItemProperty -Path -Destination -Name

        #region New-ItemProperty -Path -Name -Value

        [Fact]
        public void Powershell_creates_item_property()
        {
            // ARRANGE
            var child = new UnderlyingDictionary();
            var root = new UnderlyingDictionary
            {
                ["data"] = 1,
                ["child"] = child
            };

            this.ArrangeFileSystem(root);

            // ACT
            var _ = this.PowerShell.AddCommand("New-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "newdata")
                .AddParameter("Value", 1)
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True(root.TryGetValue("newdata", out var value));
            Assert.Equal(1, value);
        }

        #endregion New-ItemProperty -Path -Name -Value

        #region Rename-ItemProperty -Path -Name -NewName

        [Fact]
        public void Powershell_renames_item_property()
        {
            // ARRANGE
            var child = new UnderlyingDictionary();
            var root = new UnderlyingDictionary
            {
                ["data"] = 1,
                ["child"] = child
            };

            this.ArrangeFileSystem(root);

            // ACT
            var _ = this.PowerShell.AddCommand("Rename-ItemProperty")
                .AddParameter("Path", @"test:\")
                .AddParameter("Name", "data")
                .AddParameter("NewName", "newname")
                .Invoke()
                .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);
            Assert.True(root.TryGetValue("newname", out var value));
            Assert.Equal(1, value);
        }

        #endregion Rename-ItemProperty -Path -Name -NewName
    }
}