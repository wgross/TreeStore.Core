namespace TreeStore.DictionaryFS;

public sealed class DictionaryFsDriveInfo : TreeStore.Core.Providers.TreeStoreDriveInfoBase
{
    private readonly IServiceProvider rootNodeProvider;

    public DictionaryFsDriveInfo(IServiceProvider rootNodeProvider, PSDriveInfo driveInfo)
        : base(driveInfo)
    {
        this.rootNodeProvider = rootNodeProvider;
    }

    protected override IServiceProvider GetRootNodeProvider() => this.rootNodeProvider;
}