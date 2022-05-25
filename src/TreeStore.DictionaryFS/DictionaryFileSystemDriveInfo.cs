using System;
using System.Management.Automation;

namespace TreeStore.DictionaryFS
{
    public class DictionaryFileSystemDriveInfo : TreeStore.Core.Providers.TreeStoreDriveInfoBase
    {
        public DictionaryFileSystemDriveInfo(Func<string, IServiceProvider> rootNodeProvider, PSDriveInfo driveInfo)
            : base(driveInfo, rootNodeProvider)
        {
        }
    }
}