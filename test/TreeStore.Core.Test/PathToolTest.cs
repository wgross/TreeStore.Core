﻿using System.Collections.Generic;
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
            var result = PathTool.Default.SplitUnqualifiedPath("");

            // ASSERT
            Assert.Empty(result.Items);
            Assert.False(result.IsRooted);
            Assert.False(result.IsRoot);
        }

        [Fact]
        public void PathTool_splits_root_path()
        {
            // ACT
            var result = PathTool.Default.SplitUnqualifiedPath(@"\");

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
            var result = PathTool.Default.SplitUnqualifiedPath(path);

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
            var result = PathTool.Default.SplitUnqualifiedPath(path);

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
            var result = PathTool.Default.SplitDriveQualifiedPath(path);

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
            var result = PathTool.Default.SplitDriveQualifiedPath(path);

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
            var result = PathTool.Default.SplitDriveQualifiedPath(path);

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
            var result = PathTool.Default.SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Equal("module", result.Module);
            Assert.Equal("provider", result.Provider);
            Assert.Equal("drive", result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
        }

        [Theory]
        [InlineData(@"mod.ule\provider::drive:\path\to\item")]
        public void PathTool_splits_dotted_provider_qualifed_path_with_drive(string path)
        {
            // ACT
            var result = PathTool.Default.SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Equal("mod.ule", result.Module);
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
            var result = PathTool.Default.SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Equal("module", result.Module);
            Assert.Equal("provider", result.Provider);
            Assert.Null(result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
        }

        [Theory]
        [InlineData(@"drive:\path\to\item", "drive")]
        [InlineData(@"drive:\path\to\item\", "drive")]
        [InlineData(@"drive-1:\path\to\item", "drive-1")]
        public void PathTool_splits_provider_qualified_path_without_provider(string path, string driveName)
        {
            // ACT
            var result = PathTool.Default.SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Null(result.Provider);
            Assert.Equal(driveName, result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.True(result.IsRooted);
        }

        [Theory]
        [InlineData(@"\path\to\item")]
        [InlineData(@"\path\to\item\")]
        public void PathTool_splits_rooted_provider_qualified_path_without_provider_and_drive(string path)
        {
            // ACT
            var result = PathTool.Default.SplitProviderQualifiedPath(path);

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
            var result = PathTool.Default.SplitProviderQualifiedPath(path);

            // ASSERT
            Assert.Null(result.Provider);
            Assert.Null(result.DriveName);
            Assert.Equal(new[] { "path", "to", "item" }, result.Items);
            Assert.False(result.IsRooted);
        }

        public static IEnumerable<object[]> CombinedPathItems
        {
            get
            {
                yield return [@"module\provider::drive:", true, new PathString("path", "to", "item")];
                yield return [@"module\provider::drive:", false, new PathString("path", "to", "item")];
                yield return [@"drive:", true, new PathString("path", "to", "item")];
                yield return [@"drive:", false, new PathString("path", "to", "item")];
                yield return [@"", true, new PathString("path", "to", "item")];
                yield return [@"", false, new PathString("path", "to", "item")];
            }
        }

        [Theory]
        [MemberData(nameof(PathToolTest.CombinedPathItems), MemberType = typeof(PathToolTest))]
        //[InlineData(@"module\provider::drive:\path\to\item")]
        //[InlineData(@"module\provider::drive:path\to\item")]
        //[InlineData(@"drive:\path\to\item")]
        //[InlineData(@"drive:path\to\item")]
        //[InlineData(@"path\to\item")]
        public void Path_tool_combines_path_to_string(string prefix, bool isRooted, PathString path)
        {
            // ACT
            var result = PathTool.Default.SplitProviderQualifiedPath($"{prefix}{path.ToString(isRooted)}");

            // ASSERT
            Assert.Equal($"{prefix}{path.ToString(isRooted)}", result.ToPathString());
        }
    }
}