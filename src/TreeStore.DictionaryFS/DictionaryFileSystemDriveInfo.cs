using System;
using System.Management.Automation;

namespace TreeStore.DictionaryFS
{
    public class DictionaryFileSystemDriveInfo : TreeStore.Core.Providers.PowershellFileSystemDriveInfo
    {
        public DictionaryFileSystemDriveInfo(Func<string, IServiceProvider> rootNodeProvider, PSDriveInfo driveInfo)
            : base(driveInfo, rootNodeProvider)
        {
        }
    }
}