﻿using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace TreeStore.DictionaryFS.Test;

[Collection(nameof(PowerShell))]
public sealed class NavigationCmdletProviderTest : ItemCmdletProviderTestBase
{
    #region Move-Item -Path -Destination

    [Fact]
    public void PowerShell_moves_node_to_child()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new Dictionary<string, object?>
        {
            ["child1"] = new Dictionary<string, object?>(),
            ["child2"] = new Dictionary<string, object?>(),
            ["property"] = "text"
        });
        var child1 = root["child1"];

        // ACT
        this.PowerShell.AddCommand("Move-Item")
            .AddParameter("Path", @"test:\child1")
            .AddParameter("Destination", @"test:\child2")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.False(root.TryGetValue("child1", out var _));
        Assert.Same(child1, ((Dictionary<string, object?>)root["child2"]!)["child1"]);
    }

    [Fact]
    public void PowerShell_moves_node_to_child_with_provider_path()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new Dictionary<string, object?>
        {
            ["child1"] = new Dictionary<string, object?>(),
            ["child2"] = new Dictionary<string, object?>(),
            ["property"] = "text"
        });
        var child1 = root["child1"];

        // ACT
        this.PowerShell.AddCommand("Move-Item")
            .AddParameter("Path", @"TreeStore.DictionaryFS\DictionaryFS::test:\child1")
            .AddParameter("Destination", @"TreeStore.DictionaryFS\DictionaryFS::test:\child2")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.False(root.TryGetValue("child1", out var _));
        Assert.Same(child1, ((Dictionary<string, object?>)root["child2"]!)["child1"]);
    }

    [Fact]
    public void PowerShell_moves_node_to_child_with_new_name()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new Dictionary<string, object?>
        {
            ["child1"] = new Dictionary<string, object?>(),
            ["child2"] = new Dictionary<string, object?>(),
            ["property"] = "text"
        });
        var child1 = root["child1"];

        // ACT
        this.PowerShell.AddCommand("Move-Item")
            .AddParameter("Path", @"test:\child1")
            .AddParameter("Destination", @"test:\child2\newname")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.False(root.TryGetValue("child1", out var _));
        Assert.Same(child1, ((Dictionary<string, object>)root["child2"]!)["newname"]);
    }

    [Fact]
    public void PowerShell_moves_node_to_grandchild_with_new_name()
    {
        // ARRANGE
        var root = this.ArrangeFileSystem(new Dictionary<string, object?>
        {
            ["child1"] = new Dictionary<string, object?>(),
            ["child2"] = new Dictionary<string, object?>(),
            ["property"] = "text"
        });
        var child1 = root["child1"];

        // ACT
        this.PowerShell.AddCommand("Move-Item")
            .AddParameter("Path", @"test:\child1")
            .AddParameter("Destination", @"test:\child2\child3\newname")
            .Invoke()
            .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);
        Assert.False(root.TryGetValue("child1", out var _));
        Assert.Same(child1, ((Dictionary<string, object>)((Dictionary<string, object>)root["child2"]!)["child3"])["newname"]);
    }

    #endregion Move-Item -Path -Destination
}