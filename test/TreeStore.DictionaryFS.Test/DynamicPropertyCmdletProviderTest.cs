using System.Linq;
using TreeStore.Core;
using Xunit;
using UnderlyingDictionary = System.Collections.Generic.Dictionary<string, object?>;

namespace TreeStore.DictionaryFS.Test;

[Collection(nameof(PowerShell))]
public class DynamicPropertyCmdletProviderTest : ItemCmdletProviderTestBase
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

    [Fact]
    public void Powershell_copies_item_property_wizth_propvider_path()
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
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Destination", @"TreeStore.DictionaryFS\DictionaryFS::test:\child")
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
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["child"] = child
        });

        // ACT
        var result = this.PowerShell.AddCommand("Remove-ItemProperty")
            .AddParameter("Path", @"test:\")
            .AddParameter("Name", "data")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.DoesNotContain(result.Properties, p => p.Name == "data");
    }

    [Fact]
    public void Powershell_removes_item_propery_with_provider_path()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["child"] = child
        });

        // ACT
        var result = this.PowerShell.AddCommand("Remove-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Name", "data")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.DoesNotContain(result.Properties, p => p.Name == "data");
    }

    #endregion Remove-ItemProperty -Path -Name

    #region Move-ItemProperty -Path -Destination -Name

    [Fact]
    public void Powershell_moves_item_property()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["child"] = child
        });

        // ACT
        var result = this.PowerShell.AddCommand("Move-ItemProperty")
            .AddParameter("Path", @"test:\")
            .AddParameter("Destination", @"test:\child")
            .AddParameter("Name", "data")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\child")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal(2, result.Length);

        // the data property was removed from root
        Assert.DoesNotContain(result[0].Properties, p => p.Name == "data");

        // the data property was added to the child
        Assert.Equal(1, result[1].Property<int>("data"));
    }

    [Fact]
    public void Powershell_moves_item_property_with_provider_path()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["child"] = child
        });

        // ACT
        var result = this.PowerShell.AddCommand("Move-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Destination", @"TreeStore.DictionaryFS\DictionaryFS::test:\child")
            .AddParameter("Name", "data")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\child")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal(2, result.Length);

        // the data property was removed from root
        Assert.DoesNotContain(result[0].Properties, p => p.Name == "data");

        // the data property was added to the child
        Assert.Equal(1, result[1].Property<int>("data"));
    }

    #endregion Move-ItemProperty -Path -Destination -Name

    #region New-ItemProperty -Path -Name -Value

    [Fact]
    public void Powershell_creates_item_property()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["child"] = child
        });

        // ACT
        var result = this.PowerShell.AddCommand("New-ItemProperty")
            .AddParameter("Path", @"test:\")
            .AddParameter("Name", "newdata")
            .AddParameter("Value", 1)
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal(1, result.Property<int>("newdata"));
    }

    [Fact]
    public void Powershell_creates_item_property_with_provider_path()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["child"] = child
        });

        // ACT
        var result = this.PowerShell.AddCommand("New-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Name", "newdata")
            .AddParameter("Value", 1)
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal(1, result.Property<int>("newdata"));
    }

    [Fact]
    public void Powershell_creates_item_property_from_array()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["child"] = child
        });

        // ACT
        var result = this.PowerShell.AddCommand("New-ItemProperty")
            .AddParameter("Path", @"test:\")
            .AddParameter("Name", "newdata")
            .AddParameter("Value", new int[] { 1, 2, 3 })
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal(new[] { 1, 2, 3 }, result.Property<int[]>("newdata"));
    }

    #endregion New-ItemProperty -Path -Name -Value

    #region Rename-ItemProperty -Path -Name -NewName

    [Fact]
    public void Powershell_renames_item_property()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["child"] = child
        });

        // ACT
        var result = this.PowerShell
            .AddCommand("Rename-ItemProperty")
            .AddParameter("Path", @"test:\")
            .AddParameter("Name", "data")
            .AddParameter("NewName", "newname")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal(1, result.Property<int>("newname"));
    }

    [Fact]
    public void Powershell_renames_item_property_with_provider_path()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["child"] = child
        });

        // ACT
        var result = this.PowerShell
            .AddCommand("Rename-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Name", "data")
            .AddParameter("NewName", "newname")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal(1, result.Property<int>("newname"));
    }

    #endregion Rename-ItemProperty -Path -Name -NewName
}