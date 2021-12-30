using Sprache;
using System.Linq;

namespace PowerShellFilesystemProviderBase.Providers
{
    public class PathTool
    {
        private static class PathParser
        {
            //TODO: should come from the provider
            private static readonly char[] invalidPathCharacters = System.IO.Path.GetInvalidFileNameChars();

            private static readonly Parser<char> PathCharacters = Parse.CharExcept(invalidPathCharacters);

            private static readonly Parser<char> PathSeperator = Parse.Chars('\\', '/');

            // pathItem(\)
            private static readonly Parser<string> PathItem =
                from pathItem in PathCharacters.AtLeastOnce().Text()
                from _ in PathSeperator.Once().Optional()
                select pathItem;

            // (\)p\a\t\h
            public static readonly Parser<(bool isRooted, string[] items)> Path =
                from isRooted in Parse.Char('\\').Once().Optional()
                from items in PathItem.Many()
                select (isRooted.IsDefined, items.ToArray());

            // drive:
            private static readonly Parser<string> Drive =
                from drive in Parse.LetterOrDigit.AtLeastOnce().Text()
                from _1 in Parse.Char(':').Once()
                select drive;

            // Not required at the moment
            // drive:\p\a\t\h
            //public static readonly Parser<(string drive, bool isRooted, string[] path)> DriveAndPath =
            //    from drive in Drive
            //    from path in Path
            //    select (drive, isRooted: true, path.items); // a path with a drive is always rooted

            // module\provider::
            private static readonly Parser<(string module, string provider)> ModuleProvider =
                from module in Parse.LetterOrDigit.AtLeastOnce().Text()
                from _1 in Parse.Char('\\').Once()
                from provider in Parse.AtLeastOnce(Parse.LetterOrDigit).Text()
                from _2 in Parse.Char(':').Repeat(2)
                select (module, provider);

            // (module\provider::)drive: a drive is optionally prefixed with the module\provider name
            private static readonly Parser<(string module, string provider, string drive)> ModuleProviderDrive =
                from moduleProvider in ModuleProvider.Optional()
                from drive in Drive
                select (
                    module: moduleProvider.GetOrDefault().module,
                    provider: moduleProvider.GetOrDefault().provider,
                    drive: drive
                );

            // (module\provider::drive:)\p\a\t\h
            public static readonly Parser<(string module, string provider, string drive, (bool isRooted, string[] items) path)> ProviderPath =
                from moduleProviderDrive in ModuleProviderDrive.Optional()
                from path in Path
                select (
                    module: moduleProviderDrive.GetOrDefault().module,
                    provider: moduleProviderDrive.GetOrDefault().provider,
                    drive: moduleProviderDrive.GetOrDefault().drive,
                    path: (
                        isrooted: moduleProviderDrive.IsDefined ? true : path.isRooted, // a path with module/provider/drive is implicitly rooted
                        items: path.items
                    )
                );
        }

        public (string[] parentPath, string? childName) SplitParentPathAndChildName(string path)
        {
            var splitted = this.SplitProviderPath(path);
            return splitted.path.items.Any()
                ? (parentPath: splitted.path.items[0..^1], childName: splitted.path.items[^1])
                : (parentPath: new string[0], childName: null);
        }

        /// <summary>
        /// Parse a fully qualified provider path to its parts.
        /// </summary>
        /// <param name="fullyQualifiedProviderPath"></param>
        /// <returns></returns>
        public (string? module, string? provider, string? drive, (bool isRooted, string[] items) path) SplitProviderPath(string fullyQualifiedProviderPath)
        {
            if (string.IsNullOrEmpty(fullyQualifiedProviderPath))
            {
                return (null, null, null, (false, new string[0]));
            }
            else return PathParser.ProviderPath.Parse(fullyQualifiedProviderPath);
        }
    }
}