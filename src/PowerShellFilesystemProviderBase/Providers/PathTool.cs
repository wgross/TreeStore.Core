namespace PowerShellFilesystemProviderBase.Providers
{
    public class PathTool
    {
        public string[] Split(string path)
        {
            if (string.IsNullOrEmpty(path))
                return new string[0] { };

            return path.Split(separator: System.IO.Path.DirectorySeparatorChar, System.StringSplitOptions.RemoveEmptyEntries);
        }

        public (string[] parentPath, string childName) SplitParentPath(string path)
        {
            var splitted = this.Split(path);
            return (splitted[0..^1], splitted[^1]);
        }
    }
}