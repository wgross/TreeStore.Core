namespace TreeStore.Core.Providers
{
    public partial class TreeStoreCmdletProviderBase : ICmdletProvider
    {
        object ICmdletProvider.DynamicParameters => this.DynamicParameters;
    }
}