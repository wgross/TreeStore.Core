using TreeStore.Core.Providers;
using Xunit;

namespace TreeStore.Core.Test;

public class PathStringTest
{
    [Fact]
    public void Create_from_path_items()
    {
        // ACT
        var result = new PathString("a", "b", "c");

        // ASSERT
        Assert.Equal(new string[] { "a", "b", "c" }, result);
        Assert.Equal(@"a\b\c", result.ToWin32Path());
        Assert.Equal(@"a/b/c", result.ToUnixPath());
    }

    [Fact]
    public void Create_rooted_from_path_items()
    {
        // ACT
        var result = new PathString("a", "b", "c");

        // ASSERT
        Assert.Equal(new string[] { "a", "b", "c" }, result);
        Assert.Equal(@"\a\b\c", result.ToWin32Path(true));
        Assert.Equal(@"/a/b/c", result.ToUnixPath(true));
    }

    [Fact]
    public void Remove_path_separators_from_items()
    {
        // ACT
        var result = new PathString("a\\", "b/", "c");

        // ASSERT
        Assert.Equal(new string[] { "a", "b", "c" }, result);
        Assert.Equal(@"a\b\c", result.ToWin32Path());
        Assert.Equal(@"a/b/c", result.ToUnixPath());
    }

    [Fact]
    public void Ignore_null()
    {
        // ACT
        var result = new PathString("a\\", null, "b/", "", "c");

        // ASSERT
        Assert.Equal(new string[] { "a", "b", "c" }, result);
        Assert.Equal(@"a\b\c", result.ToWin32Path());
        Assert.Equal(@"a/b/c", result.ToUnixPath());
    }
}