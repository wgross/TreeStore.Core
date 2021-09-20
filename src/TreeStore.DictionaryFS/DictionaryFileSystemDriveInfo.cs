using System;
using System.Management.Automation;

namespace TreeStore.DictionaryFS
{
    public class DictionaryFileSystemDriveInfo : PowerShellFilesystemProviderBase.Providers.PowershellFileSystemDriveInfo
    {
        public DictionaryFileSystemDriveInfo(Func<string, IServiceProvider> rootNodeProvider, PSDriveInfo driveInfo)
            : base(driveInfo, rootNodeProvider)
        {
        }
    }
}