using System;
using System.Management.Automation;

namespace TreeStore.DictionaryFS;

public sealed class DictionaryFsDriveInfo : TreeStore.Core.Providers.TreeStoreDriveInfoBase
{
    private readonly Func<string, IServiceProvider> rootNodeProvider;

    public DictionaryFsDriveInfo(Func<string, IServiceProvider> rootNodeProvider, PSDriveInfo driveInfo)
        : base(driveInfo)
    {
        this.rootNodeProvider = rootNodeProvider;
    }

    protected override IServiceProvider GetRootNodeProvider() => this.rootNodeProvider(this.Name);
}