namespace TreeStore.DictionaryFS;

public sealed class DictionaryFsDriveInfo(IServiceProvider rootNodeProvider, PSDriveInfo driveInfo) : TreeStoreDriveInfoBase(driveInfo)
{
    private readonly IServiceProvider rootNodeProvider = rootNodeProvider;

    protected override IServiceProvider GetRootNodeProvider() => this.rootNodeProvider;
}