﻿using Superpower;
using Superpower.Parsers;
using System.Text;

namespace TreeStore.Core.Providers;

public record UnqualifiedPath(bool IsRooted, string[] Items)
{
    public (string[] Parent, string? Child) ParentAndChild => this.Items.Length switch
    {
        0 => (Array.Empty<string>(), null),
        1 => (Array.Empty<string>(), this.Items[0]),
        _ => (this.Items[0..^1], this.Items[^1])
    };

    public bool IsRoot => this.IsRooted && this.Items.Length == 0;
}

public record DriveQualifedPath(string? DriveName, bool IsRooted, string[] Items) : UnqualifiedPath(IsRooted, Items);

public record ProviderName(string Module, string Name);

/// <summary>
/// Represents a fully qualified PowerShell path
/// </summary>
/// <param name="Module">Name of the PowerShell module providing the cmdlet provider (may be null)</param>
/// <param name="Provider">Name of the PowerShell cmdlet provider (may be null)</param>
/// <param name="DriveName">Drive name (may be null)</param>
/// <param name="IsRooted">Indicates if the path is rooted (starts wit / or \) or not</param>
/// <param name="Items">the split path items; may be empty</param>
public record ProviderQualifiedPath(string? Module, string? Provider, string? DriveName, bool IsRooted, string[] Items)
    : DriveQualifedPath(DriveName, IsRooted, Items)
{
    public string ToPathString()
    {
        return new StringBuilder()
            .Append(this.Module is not null ? $"{this.Module}\\{this.Provider}::" : string.Empty)
            .Append(this.DriveName is not null ? $"{this.DriveName}:" : string.Empty)
            .Append(new PathString(this.Items).ToString(this.IsRooted))
            .ToString();
    }
}

public sealed class PathTool
{
    /// <summary>
    /// Singleton to use
    /// </summary>
    public static PathTool Default = new();

    #region Path items

    /// <summary>
    /// PowerSHell seems to be confused by file names containing special characters like ':' of '\'
    /// while only '/' is fobidden on unix.
    /// Therefore i'm using the forbidden chars of windows everywhere as long as it works better
    /// </summary>
    private static readonly char[] InvalidFileNameChars = new char[41]
    {
        '"', '<', '>', '|', '\0', '\u0001', '\u0002', '\u0003', '\u0004', '\u0005',
        '\u0006', '\a', '\b', '\t', '\n', '\v', '\f', '\r', '\u000e', '\u000f',
        '\u0010', '\u0011', '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018', '\u0019',
        '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f', ':', '*', '?', '\\',
        '/'
    };

    // module name may contain a dot.
    private static readonly TextParser<char> ParseModuleNameCharacters
        = Character.Matching(c => char.IsLetterOrDigit(c) || c == '.', nameof(ParseModuleNameCharacters));

    // Define characters that are allowed in a path
    private static readonly TextParser<char> ParsePathCharacters
        // = Character.ExceptIn(System.IO.Path.GetInvalidFileNameChars());
        = Character.ExceptIn(InvalidFileNameChars);

    // path items are separated by / or \
    private static readonly TextParser<char> ParsePathSeparator = Character.In('\\', '/');

    /// <summary>
    /// A path item is a collection of <see cref="ParsePathCharacters"/> followed by
    /// a <see cref="ParsePathSeparator"/>
    /// </summary>
    private static readonly TextParser<string> ParsePathItem =
       from pathItem in ParsePathCharacters.AtLeastOnce()
       from _ in ParsePathSeparator.Optional() // the last item isn't followed by a /
       select new string(pathItem);

    /// <summary>
    /// A path consists of multiple <see cref="ParsePathItem"/>
    /// </summary>
    private static readonly TextParser<string[]> ParsePathItems =
         from items in ParsePathItem.Many()
         select items.ToArray();

    public static readonly TextParser<(bool isRooted, string[] items)> ParseRootedPath =
        from rootPath in ParsePathSeparator.Optional()
        from items in ParsePathItems
        select (rootPath.HasValue, items);

    private static readonly TextParser<string?> ParseDriveName =
           from drive in ParsePathCharacters.AtLeastOnce()
           from _ in Character.EqualTo(':')
           select new string(drive);

    private static readonly TextParser<(string? module, string? provider)> ParseProviderName =
        from module in ParseModuleNameCharacters.AtLeastOnce().Named("module name")
        from _1 in ParsePathSeparator.Repeat(1)
        from provider in Character.LetterOrDigit.AtLeastOnce().Named("provider name")
        from _2 in Character.EqualTo(':').Repeat(2)
        select (new string(module), new string(provider));

    #endregion Path items

    private static readonly TextParser<ProviderQualifiedPath> ParseProviderQualifiedPath =
       from provider in ParseProviderName.Try().OptionalOrDefault((module: null, provider: null))
       from driveName in ParseDriveName.Try().OptionalOrDefault(null)
       from path in ParseRootedPath
       select new ProviderQualifiedPath(provider.module, provider.provider, driveName, path.isRooted, path.items);

    /// <summary>
    /// Splits a path that may be fully qualified with provider name and module name in its parts
    /// </summary>
    public ProviderQualifiedPath SplitProviderQualifiedPath(string path)
        => ParseProviderQualifiedPath.Parse(path);

    /// <summary>
    /// Splits a path which doesn't have provider and drive qualification in it
    /// </summary>
    public UnqualifiedPath SplitUnqualifiedPath(string path)
        => ParseProviderQualifiedPath.Parse(path);

    /// <summary>
    /// Splits a path which doesn't have provider bit may have a drive qualification in it.
    /// </summary>
    public DriveQualifedPath SplitDriveQualifiedPath(string path)
        => ParseProviderQualifiedPath.Parse(path);
}