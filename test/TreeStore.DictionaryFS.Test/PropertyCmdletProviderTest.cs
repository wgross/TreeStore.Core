using System.Linq;
using System.Management.Automation;
using TreeStore.Core;
using Xunit;
using UnderlyingDictionary = System.Collections.Generic.Dictionary<string, object?>;

namespace TreeStore.DictionaryFS.Test;

[Collection(nameof(PowerShell))]
public class PropertyCmdletProviderTest : ItemCmdletProviderTestBase
{
    #region New-ItemProperty -Path -Name -Value

    [Fact]
    public void Powershell_creates_item_property()
    {
        // ARRANGE
        var root = new UnderlyingDictionary();

        this.ArrangeFileSystem(root);

        // ACT
        var result = this.PowerShell.AddCommand("New-ItemProperty")
            .AddParameter("Path", @"test:\")
            .AddParameter("Name", "data")
            .AddParameter("Value", "text")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal("text", result.Property<string>("data"));
    }

    [Fact]
    public void Powershell_creates_item_property_with_provider_path()
    {
        // ARRANGE
        var root = new UnderlyingDictionary();

        this.ArrangeFileSystem(root);

        // ACT
        var result = this.PowerShell.AddCommand("New-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Name", "data")
            .AddParameter("Value", "text")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal("text", result.Property<string>("data"));
    }

    #endregion New-ItemProperty -Path -Name -Value

    #region Clear-ItemProperty -Path -Name

    [Fact]
    public void PowerShell_clears_item_property_value()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1
        });

        // ACT
        var result = this.PowerShell
            .AddCommand("Clear-ItemProperty")
            .AddParameter("Path", @"test:\")
            .AddParameter("Name", "data")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        // property is gone
        Assert.False(this.PowerShell.HadErrors);
        Assert.Null(result.Property<object>("data"));
    }

    [Fact]
    public void PowerShell_clears_item_property_value_with_provider_path()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1
        });

        // ACT
        var result = this.PowerShell
            .AddCommand("Clear-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Name", "data")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        // property is gone
        Assert.False(this.PowerShell.HadErrors);
        Assert.Null(result.Property<object>("data"));
    }

    #endregion Clear-ItemProperty -Path -Name

    #region Get-ItemProperty -Path -Name

    [Fact]
    public void Powershell_gets_item_property()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["data_skipped"] = 2
        });

        // ACT
        var result = this.PowerShell.AddCommand("Get-ItemProperty")
            .AddParameter("Path", @"test:\")
            .AddParameter("Name", "data")
            .Invoke()
            .Single();

        // ASSERT
        // an object having the requested property only was returned
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal(1, result.Property<int>("data"));
        Assert.True(result.Property<bool>("PSIsContainer"));
        Assert.Equal("test", result.Property<PSDriveInfo>("PSDrive").Name);
        Assert.Equal("DictionaryFS", result.Property<ProviderInfo>("PSProvider").Name);
        Assert.Equal(@"TreeStore.DictionaryFS\DictionaryFS::test:\", result.Property<string>("PSPath"));
        Assert.Equal("", result.Property<string>("PSParentPath"));

        // the property data_skipped is missing in the result
        Assert.DoesNotContain(result.Properties, p => p.Name == "data_skipped");
    }

    public void Powershell_gets_item_property_with_provider_path()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1,
            ["data_skipped"] = 2
        });

        // ACT
        var result = this.PowerShell.AddCommand("Get-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Name", "data")
            .Invoke()
            .Single();

        // ASSERT
        // an object having the requested property only was returned
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal(1, result.Property<int>("data"));
        Assert.True(result.Property<bool>("PSIsContainer"));
        Assert.True(result.PropertyIsNull("PSDrive"));
        Assert.Equal("DictionaryFS", result.Property<ProviderInfo>("PSProvider").Name);
        Assert.Equal(@"TreeStore.DictionaryFS\DictionaryFS::test:\", result.Property<string>("PSPath"));
        Assert.Equal("", result.Property<string>("PSParentPath"));

        // the property data_skipped is missing in the result
        Assert.DoesNotContain(result.Properties, p => p.Name == "data_skipped");
    }

    #endregion Get-ItemProperty -Path -Name

    #region Set-ItemProperty -Path -Name

    [Fact]
    public void Powershell_sets_item_property()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1
        });

        // ACT
        var result = this.PowerShell.AddCommand("Set-ItemProperty")
            .AddParameter("Path", @"test:\")
            .AddParameter("Name", "data")
            .AddParameter("Value", "text")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        // value has changed
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal("text", result.Property<string>("data"));
    }

    [Fact]
    public void Powershell_sets_item_property_with_provider_path()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["data"] = 1
        });

        // ACT
        var result = this.PowerShell.AddCommand("Set-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Name", "data")
            .AddParameter("Value", "text")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        // value has changed
        Assert.False(this.PowerShell.HadErrors);
        Assert.Equal("text", result.Property<string>("data"));
    }

    #endregion Set-ItemProperty -Path -Name

    #region Rename-ItemProperty -Path

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
        var _ = this.PowerShell.AddCommand("Rename-ItemProperty")
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
    public void Powershell_renames_item_property_with_provider_path()
    {
        // ARRANGE
        var root = new UnderlyingDictionary
        {
            ["data"] = 1
        };

        this.ArrangeFileSystem(root);

        // ACT
        var _ = this.PowerShell.AddCommand("Rename-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
            .AddParameter("Name", "data")
            .AddParameter("NewName", "data-changed")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.True(root.TryGetValue("data-changed", out var value));
        Assert.Equal(1, value);
    }

    #endregion Rename-ItemProperty -Path

    #region Copy-ItemProperty -Path

    [Fact]
    public void Powershell_copies_item_property()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();

        var root = new UnderlyingDictionary
        {
            ["child"] = child,
            ["data"] = 1
        };

        this.ArrangeFileSystem(root);

        // ACT
        var _ = this.PowerShell.AddCommand("Copy-ItemProperty")
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
    public void Powershell_copies_item_property_with_provider_name()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();

        var root = new UnderlyingDictionary
        {
            ["child"] = child,
            ["data"] = 1
        };

        this.ArrangeFileSystem(root);

        // ACT
        var _ = this.PowerShell.AddCommand("Copy-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
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

    #endregion Copy-ItemProperty -Path

    #region Move-ItemProperty -Path

    [Fact]
    public void Powershell_moves_item_property()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();

        var root = new UnderlyingDictionary
        {
            ["child"] = child,
            ["data"] = 1
        };

        this.ArrangeFileSystem(root);

        // ACT
        var _ = this.PowerShell.AddCommand("Move-ItemProperty")
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

    [Fact]
    public void Powershell_moves_item_property_with_provider_path()
    {
        // ARRANGE
        var child = new UnderlyingDictionary();

        var root = new UnderlyingDictionary
        {
            ["child"] = child,
            ["data"] = 1
        };

        this.ArrangeFileSystem(root);

        // ACT
        var _ = this.PowerShell.AddCommand("Move-ItemProperty")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\")
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

    #endregion Move-ItemProperty -Path
}