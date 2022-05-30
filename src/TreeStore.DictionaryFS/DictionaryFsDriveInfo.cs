using System;
using System.Management.Automation;

namespace TreeStore.DictionaryFS;

public sealed class DictionaryFsDriveInfo : TreeStore.Core.Providers.TreeStoreDriveInfoBase
{
    public DictionaryFsDriveInfo(Func<string, IServiceProvider> rootNodeProvider, PSDriveInfo driveInfo)
        : base(driveInfo, rootNodeProvider)
    {
    }
}
