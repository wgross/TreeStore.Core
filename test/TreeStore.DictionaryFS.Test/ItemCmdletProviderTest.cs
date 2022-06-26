using System.Linq;
using System.Management.Automation;
using TreeStore.Core;
using Xunit;
using UnderlyingDictionary = System.Collections.Generic.Dictionary<string, object?>;

namespace TreeStore.DictionaryFS.Test;

[Collection(nameof(PowerShell))]
public sealed class ItemCmdletProviderTest : ItemCmdletProviderTestBase
{
    #region Get-Item -Path

    [Fact]
    public void Powershell_reads_root_node()
    {
        // ARRANGE
        this.ArrangeFileSystem(new UnderlyingDictionary());

        // ACT
        var result = this.PowerShell.AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);

        var psobject = result.Single();

        Assert.Equal("test:", psobject.Property<string>("PSChildName"));
        Assert.True(psobject.Property<bool>("PSIsContainer"));
        Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
        Assert.Equal("DictionaryFS", psobject.Property<ProviderInfo>("PSProvider").Name);
        Assert.Equal(@"TreeStore.DictionaryFS\DictionaryFS::test:\", psobject.Property<string>("PSPath"));
        Assert.Equal(string.Empty, psobject.Property<string>("PSParentPath"));
    }

    [Fact]
    public void Powershell_reads_root_child_node()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["item"] = new UnderlyingDictionary()
        });

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
        Assert.Equal("DictionaryFS", psobject.Property<ProviderInfo>("PSProvider").Name);
        Assert.Equal(@"TreeStore.DictionaryFS\DictionaryFS::test:\item", psobject.Property<string>("PSPath"));
        Assert.Equal(@"TreeStore.DictionaryFS\DictionaryFS::test:", psobject.Property<string>("PSParentPath"));
    }

    [Fact]
    public void Powershell_reads_root_grandchild_node()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["item"] = new UnderlyingDictionary
            {
                ["item"] = new UnderlyingDictionary()
            }
        });

        // ACT
        var result = this.PowerShell.AddCommand("Get-Item")
            .AddParameter("Path", @"test:\item\item")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);

        var psobject = result.Single();

        Assert.Equal("item", psobject.Property<string>("PSChildName"));
        Assert.True(psobject.Property<bool>("PSIsContainer"));
        Assert.Equal("test", psobject.Property<PSDriveInfo>("PSDrive").Name);
        Assert.Equal("DictionaryFS", psobject.Property<ProviderInfo>("PSProvider").Name);
        Assert.Equal(@"TreeStore.DictionaryFS\DictionaryFS::test:\item\item", psobject.Property<string>("PSPath"));
        Assert.Equal(@"TreeStore.DictionaryFS\DictionaryFS::test:\item", psobject.Property<string>("PSParentPath"));
    }

    #endregion Get-Item -Path

    #region Set-Item -Path

    [Fact]
    public void Powershell_sets_item_value()
    {
        // ARRANGE

        var root = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["child"] = new UnderlyingDictionary()
        });

        var newValue = new UnderlyingDictionary
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
        Assert.Same(newValue["data"], ((UnderlyingDictionary)root["child"]!)["data"]);
    }

    #endregion Set-Item -Path

    #region Clear-Item -Path

    [Fact]
    public void Powershell_clears_item_value()
    {
        // ARRANGE
        var node = this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["child"] = new object()
        });

        // ACT
        var result = this.PowerShell.AddCommand("Clear-Item")
            .AddParameter("Path", @"test:\")
            .AddStatement()
            .AddCommand("Get-Item")
            .AddParameter("Path", @"test:\")
            .Invoke()
            .Single();

        // ASSERT
        // underlying dictionary is empty
        Assert.False(this.PowerShell.HadErrors);
        Assert.True(result.PropertyIsNull("child"));
    }

    #endregion Clear-Item -Path

    #region Test-Path -Path -PathType

    [Fact]
    public void Powershell_tests_root_path()
    {
        // ARRANGE
        this.ArrangeFileSystem(new UnderlyingDictionary());

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
    public void Powershell_tests_root_path_as_container()
    {
        // ARRANGE
        this.ArrangeFileSystem(new UnderlyingDictionary());

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
    public void Powershell_tests_root_path_as_leaf_fails()
    {
        // ARRANGE
        this.ArrangeFileSystem(new UnderlyingDictionary());

        // ACT
        var result = this.PowerShell.AddCommand("Test-Path")
            .AddParameter("Path", @"test:\")
            .AddParameter("PathType", "Leaf")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.False((bool)result.Single().BaseObject);
    }

    [Fact]
    public void Powershell_tests_child_path_as_container()
    {
        // ARRANGE
        this.ArrangeFileSystem(new UnderlyingDictionary
        {
            ["child"] = new UnderlyingDictionary()
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
        var root = new UnderlyingDictionary
        {
            ["item"] = new object()
        };

        this.ArrangeFileSystem(root);

        // ACT
        var _ = this.PowerShell.AddCommand("Invoke-Item")
            .AddParameter("Path", @"test:\item")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.True(this.PowerShell.HadErrors);
    }

    #endregion Invoke-Item -Path
}