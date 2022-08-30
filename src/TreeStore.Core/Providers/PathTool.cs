using Superpower;
using Superpower.Parsers;

namespace TreeStore.Core.Providers;

public record UnqualifiedPath(bool IsRooted, string[] Items)
{
    public (string[] Parent, string Child) ParentAndChild => this.Items.Length switch
    {
        0 => (Array.Empty<string>(), null),
        1 => (Array.Empty<string>(), this.Items[0]),
        _ => (this.Items[0..^1], this.Items[^1])
    };

    public bool IsRoot => this.IsRooted && this.Items.Length == 0;
}

public record DriveQualifedPath(string? DriveName, bool IsRooted, string[] Items) : UnqualifiedPath(IsRooted, Items);

public record ProviderName(string Module, string Name);
public record ProviderQualifiedPath(string? Module, string? Provider, string? DriveName, bool IsRooted, string[] Items)
    : DriveQualifedPath(DriveName, IsRooted, Items);

public sealed class PathTool
{
    #region Path items

    // Define characters that are allowed in a path
    private static readonly TextParser<char> ParsePathCharacters
        = Character.ExceptIn(System.IO.Path.GetInvalidFileNameChars());

    // path items are separated by / or \
    private static readonly TextParser<char> ParsePathSeperator = Character.In('\\', '/');

    /// <summary>
    /// A path item is a collection of <see cref="ParsePathCharacters"/> followed by
    /// a <see cref="ParsePathSeperator"/>
    /// </summary>
    private static readonly TextParser<string> ParsePathItem =
       from pathItem in ParsePathCharacters.AtLeastOnce()
       from _ in ParsePathSeperator.Optional() // the last item isn't followed by a /
       select new string(pathItem);

    /// <summary>
    /// A path consists of multiple <see cref="ParsePathItem"/>
    /// </summary>
    private static readonly TextParser<string[]> ParsePathItems =
         from items in ParsePathItem.Many()
         select items.ToArray();

    private static readonly TextParser<(bool isRooted, string[] items)> ParseRootedPath =
        from rootPath in ParsePathSeperator.Optional()
        from items in ParsePathItems
        select (rootPath.HasValue, items);

    private static readonly TextParser<string> ParseDriveName =
           from drive in Character.LetterOrDigit.AtLeastOnce()
           from _ in Character.EqualTo(':')
           select new string(drive);

    private static readonly TextParser<(string module, string provider)> ParseProviderName =
        from module in Character.LetterOrDigit.AtLeastOnce().Named("module name")
        from _1 in ParsePathSeperator.Repeat(1)
        from provider in Character.LetterOrDigit.AtLeastOnce().Named("provider name")
        from _2 in Character.EqualTo(':').Repeat(2)
        select (new string(module), new string(provider));

    #endregion Path items

    private static readonly TextParser<ProviderQualifiedPath> ParseProviderQualifiedPath =
       from provider in ParseProviderName.Try().OptionalOrDefault((module: null, provider: null))
       from driveName in ParseDriveName.Try().OptionalOrDefault(null)
       from path in ParseRootedPath
       select new ProviderQualifiedPath(provider.module, provider.provider, driveName, path.isRooted, path.items);

    public ProviderQualifiedPath SplitProviderQualifiedPath(string path)
        => ParseProviderQualifiedPath.Parse(path);

    public UnqualifiedPath SplitUnqualifiedPath(string path)
        => ParseProviderQualifiedPath.Parse(path);

    public DriveQualifedPath SplitDriveQualifiedPath(string path)
        => ParseProviderQualifiedPath.Parse(path);
}