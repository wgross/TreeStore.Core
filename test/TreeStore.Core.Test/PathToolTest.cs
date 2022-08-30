using TreeStore.Core.Providers;
using Xunit;

namespace TreeStore.Core.Test
{
    public class PathToolTest
    {
        [Fact]
        public void PathTool_splits_empty_path()
        {
            // ACT
            var result = new PathTool().SplitUnqualifiedPath("");

            // ASSERT
            Assert.Empty(result.Items);
            Assert.False(result.IsRooted);
            Assert.False(result.IsRoot);
        }

        [Fact]
        public void PathTool_splits_root_path()
        {
            // ACT
            var result = new PathTool().SplitUnqualifiedPath(@"\");

            // ASSERT
            Assert.Empty(result.Items);
            Assert.True(result.IsRooted);
            Assert.True(result.IsRoot);
        }

        [Theory]
        [InlineData(@"\path\to\item\")]
        [InlineData(@"\path\to\item")]
        public void PathTool_splits_rooted_path(string path)
        {
            // ACT
            var result = new PathTool().SplitUnqualifiedPath(path);

            // ASSERT
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
            Assert.Equal(new[] { "path", "to" }, result.ParentAndChild.Parent);
            Assert.Equal("item", result.ParentAndChild.Child);
        }

        [Theory]
        [InlineData(@"path\to\item\")]
        [InlineData(@"path\to\item")]
        public void PathTool_splits_not_rooted_path(string path)
        {
            // ACT
            var result = new PathTool().SplitUnqualifiedPath(path);

            // ASSERT
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.False(result.IsRooted);
            Assert.Equal(new[] { "path", "to" }, result.ParentAndChild.Parent);
            Assert.Equal("item", result.ParentAndChild.Child);
        }

        [Theory]
        [InlineData(@"drive:\path\to\item")]
        [InlineData(@"drive:\path\to\item\")]
        [InlineData(@"module\provider::drive:\path\to\item\")]
        public void PathTool_splits_drive_qualified_path(string path)
        {
            // ACT
            var result = new PathTool().SplitDriveQualifiedPath(path);

            // ASSERT
            Assert.Equal("drive", result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
        }

        [Theory]
        [InlineData(@"\path\to\item")]
        [InlineData(@"\path\to\item\")]
        [InlineData(@"module\provider::\path\to\item\")]
        public void PathTool_splits_drive_qualified_rooted_path_without_drive(string path)
        {
            // ACT
            var result = new PathTool().SplitDriveQualifiedPath(path);

            // ASSERT
            Assert.Null(result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
        }

        [Theory]
        [InlineData(@"path\to\item")]
        [InlineData(@"path\to\item\")]
        public void PathTool_splits_drive_qualified_path_without_drive(string path)
        {
            // ACT
            var result = new PathTool().SplitDriveQualifiedPath(path);

            // ASSERT
            Assert.Null(result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.False(result.IsRooted);
        }

        [Theory]
        [InlineData(@"module\provider::drive:\path\to\item")]
        [InlineData(@"module\provider::drive:\path\to\item\")]
        public void PathTool_splits_provider_qualifed_path_with_drive(string path)
        {
            // ACT
            var result = new PathTool().SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Equal("module", result.Module);
            Assert.Equal("provider", result.Provider);
            Assert.Equal("drive", result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
        }

        [Theory]
        [InlineData(@"module\provider::\path\to\item")]
        [InlineData(@"module\provider::\path\to\item\")]
        public void PathTool_splits_provider_qualifed_path_with_provider_without_drive(string path)
        {
            // ACT
            var result = new PathTool().SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Equal("module", result.Module);
            Assert.Equal("provider", result.Provider);
            Assert.Null(result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
        }

        [Theory]
        [InlineData(@"drive:\path\to\item")]
        [InlineData(@"drive:\path\to\item\")]
        public void PathTool_splits_provider_qualified_path_without_provider(string path)
        {
            // ACT
            var result = new PathTool().SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Null(result.Provider);
            Assert.Equal("drive", result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
        }

        [Theory]
        [InlineData(@"\path\to\item")]
        [InlineData(@"\path\to\item\")]
        public void PathTool_splits_rooted_provider_qualified_path_without_provider_and_drive(string path)
        {
            // ACT
            var result = new PathTool().SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Null(result.Provider);
            Assert.Null(result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
        }

        [Theory]
        [InlineData(@"path\to\item")]
        [InlineData(@"path\to\item\")]
        public void PathTool_splits_provider_qualified_path_without_provider_and_drive(string path)
        {
            // ACT
            var result = new PathTool().SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Null(result.Provider);
            Assert.Null(result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.False(result.IsRooted);
        }
    }
}