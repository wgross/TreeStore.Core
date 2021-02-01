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
    }
}