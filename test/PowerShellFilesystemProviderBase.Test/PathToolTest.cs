using PowerShellFilesystemProviderBase.Providers;
using System.Linq;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test
{
    public class PathToolTest
    {
        //[Fact]
        //public void PathTool_splits_path_with_multiple_items()
        //{
        //    // ARRANGE
        //    var pathTool = new PathTool();

        //    // ACT
        //    var result = pathTool.Split(@"a\b");

        //    // ASSERT
        //    Assert.Equal(new[] { "a", "b" }, result);
        //}

        //[Fact]
        //public void PathTool_splits_path_with_single_item()
        //{
        //    // ARRANGE
        //    var pathTool = new PathTool();

        //    // ACT
        //    var result = pathTool.Split(@"a");

        //    // ASSERT
        //    Assert.Equal(new[] { "a" }, result);
        //}

        //[Fact]
        //public void PathTool_splitting_null_path_path_throws()
        //{
        //    // ARRANGE
        //    var pathTool = new PathTool();

        //    // ACT
        //    var result = pathTool.Split(null);

        //    // ASSERT
        //    Assert.Empty(result);
        //}

        [Fact]
        public void PathTool_splits_empty()
        {
            // ACT
            var result = new PathTool().SplitProviderPath("");

            // ASSERT
            Assert.Null(result.module);
            Assert.Null(result.provider);
            Assert.Null(result.drive);
            Assert.False(result.path.isRooted);
            Assert.Empty(result.path.items);
        }

        [Fact]
        public void PathTool_splits_provider_path()
        {
            // ACT
            var result = new PathTool().SplitProviderPath(@"ProviderModule\ProviderName::driveName:\parent\child");

            // ASSERT
            Assert.Equal("ProviderModule", result.module);
            Assert.Equal("ProviderName", result.provider);
            Assert.Equal("driveName", result.drive);
            Assert.True(result.path.isRooted);
            Assert.Equal(new[] { "parent", "child" }, result.path.items);
        }

        #region SplitProviderPath

        [Theory]
        [InlineData(@"ProviderModule\ProviderName::driveName:\")]
        [InlineData(@"ProviderModule\ProviderName::driveName:")]
        public void PathTool_splits_provider_root_path(string path)
        {
            // ACT
            var result = new PathTool().SplitProviderPath(path);

            // ASSERT
            Assert.Equal("ProviderModule", result.module);
            Assert.Equal("ProviderName", result.provider);
            Assert.Equal("driveName", result.drive);
            Assert.True(result.path.isRooted);
            Assert.Empty(result.path.items);
        }

        [Theory]
        [InlineData(@"ProviderModule\ProviderName::driveName:\child\")]
        [InlineData(@"ProviderModule\ProviderName::driveName:\child")]
        public void PathTool_splits_provider_child_path(string path)
        {
            // ACT
            var result = new PathTool().SplitProviderPath(path);

            // ASSERT
            Assert.Equal("ProviderModule", result.module);
            Assert.Equal("ProviderName", result.provider);
            Assert.Equal("driveName", result.drive);
            Assert.True(result.path.isRooted);
            Assert.Equal("child", result.path.items.Single());
        }

        [Theory]
        [InlineData(@"driveName:\child\")]
        [InlineData(@"driveName:\child")]
        public void PathTool_splits_drive_child_path(string path)
        {
            // ACT
            var result = new PathTool().SplitProviderPath(path);

            // ASSERT
            Assert.Null(result.module);
            Assert.Null(result.provider);
            Assert.Equal("driveName", result.drive);
            Assert.True(result.path.isRooted);
            Assert.Equal("child", result.path.items.Single());
        }

        [Theory]
        [InlineData(@"driveName:\")]
        [InlineData(@"driveName:")]
        public void PathTool_splits_drive_root_path(string path)
        {
            // ACT
            var result = new PathTool().SplitProviderPath(path);

            // ASSERT
            Assert.Null(result.module);
            Assert.Null(result.provider);
            Assert.Equal("driveName", result.drive);
            Assert.True(result.path.isRooted);
            Assert.Empty(result.path.items);
        }

        [Theory]
        [InlineData(@"\", true)]
        [InlineData(@"", false)]
        public void PathTool_splits_root_path(string path, bool isRooted)
        {
            // ACT
            var result = new PathTool().SplitProviderPath(path);

            // ASSERT
            Assert.Null(result.module);
            Assert.Null(result.provider);
            Assert.Null(result.drive);
            Assert.Equal(isRooted, result.path.isRooted);
            Assert.Empty(result.path.items);
        }

        [Theory]
        [InlineData(@"\child\", true)]
        [InlineData(@"child\", false)]
        [InlineData(@"child", false)]
        public void PathTool_splits_child_path(string path, bool isRooted)
        {
            // ACT
            var result = new PathTool().SplitProviderPath(path);

            // ASSERT
            Assert.Null(result.module);
            Assert.Null(result.provider);
            Assert.Null(result.drive);
            Assert.Equal(isRooted, result.path.isRooted);
            Assert.Equal("child", result.path.items.Single());
        }

        #endregion SplitProviderPath

        [Theory]
        [InlineData(@"ProviderModule\ProviderName::driveName:\")]
        [InlineData(@"ProviderModule\ProviderName::driveName:")]
        public void PathTool_splits_provider_parent_path_from_root_path(string path)
        {
            // ACT
            var result = new PathTool().SplitParentPathAndChildName(path);

            // ASSERT
            Assert.Empty(result.parentPath);
            Assert.Null(result.childName);
        }

        [Theory]
        [InlineData(@"ProviderModule\ProviderName::driveName:\child")]
        [InlineData(@"ProviderModule\ProviderName::driveName:\child\")]
        public void PathTool_splits_provider_parent_path_from_child_path(string path)
        {
            // ACT
            var result = new PathTool().SplitParentPathAndChildName(path);

            // ASSERT
            Assert.Empty(result.parentPath);
            Assert.Equal("child", result.childName);
        }

        [Theory]
        [InlineData(@"ProviderModule\ProviderName::driveName:\child\grandchild")]
        [InlineData(@"ProviderModule\ProviderName::driveName:\child\grandchild\")]
        public void PathTool_splits_provider_parent_path_from_grandchild_path(string path)
        {
            // ACT
            var result = new PathTool().SplitParentPathAndChildName(path);

            // ASSERT
            Assert.Equal(new[] { "child" }, result.parentPath);
            Assert.Equal("grandchild", result.childName);
        }
    }
}