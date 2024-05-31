using System.Collections;

namespace TreeStore.Core.Providers;

public sealed class PathString(params string[] pathItems) : IEnumerable<string>
{
    private readonly List<string> pathItemsList = new(Cleansed(pathItems));

    private static IEnumerable<string> Cleansed(IEnumerable<string> items)
        => items.Where(i => !string.IsNullOrEmpty(i)).Select(i => i.Replace("\\", "").Replace("/", ""));

    public IEnumerator<string> GetEnumerator() => this.pathItemsList.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public string ToWin32Path(bool isRooted = false) => isRooted ? string.Join("\\", ["", .. this]) : string.Join("\\", this);

    public string ToUnixPath(bool isRooted = false) => isRooted ? string.Join("/", ["", .. this]) : string.Join("/", this);

    public override string ToString() => Environment.OSVersion.Platform == PlatformID.Win32NT ? this.ToWin32Path(false) : this.ToUnixPath(false);

    public string ToString(bool isRooted) => Environment.OSVersion.Platform == PlatformID.Win32NT ? this.ToWin32Path(isRooted) : this.ToUnixPath(isRooted);
}