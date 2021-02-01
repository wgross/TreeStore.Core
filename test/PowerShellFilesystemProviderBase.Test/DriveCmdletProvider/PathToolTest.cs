using PowerShellFilesystemProviderBase.Providers;
using Xunit;

namespace PowerShellFilesystemProviderBase.Test.DriveCmdletProvider
{
    public class PathToolTest
    {
        [Fact]
        public void PathTool_splits_path_with_multiple_items()
        {
            // ARRANGE
            var pathTool = new PathTool();

            // ACT
            var result = pathTool.Split(@"a\b");

            // ASSERT
            Assert.Equal(new[] { "a", "b" }, result);
        }

        [Fact]
        public void PathTool_splits_path_with_sigle_item()
        {
            // ARRANGE
            var pathTool = new PathTool();

            // ACT
            var result = pathTool.Split(@"a");

            // ASSERT
            Assert.Equal(new[] { "a" }, result);
        }

        [Fact]
        public void PathTool_splitting_null_path_path_throws()
        {
            // ARRANGE
            var pathTool = new PathTool();

            // ACT
            var result = pathTool.Split(null);

            // ASSERT
            Assert.Empty(result);
        }
    }
}