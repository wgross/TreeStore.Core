namespace TreeStore.DictionaryFS;

/// <summary>
/// Publishes the command provider for Dictionary FS to the PowerShell. The provider name will be <see cref="DictionaryFsCmdletProvider.Id"/>.
/// It won't provide any additional capabilities (<see cref="ProviderCapabilities.None"/>)
/// </summary>
[CmdletProvider(DictionaryFsCmdletProvider.Id, ProviderCapabilities.None)]
public sealed class DictionaryFsCmdletProvider : TreeStoreCmdletProviderBase
{
    public const string Id = "DictionaryFS";

    /// <summary>
    /// Creates the root node. The input string is the drive name.
    /// </summary>
    public static Func<string, IServiceProvider>? RootNodeProvider { get; set; }

    /// <summary>
    /// Creates a new drive from the given creation parameters in <paramref name="drive"/>.
    /// </summary>
    protected override PSDriveInfo NewDrive(PSDriveInfo drive)
    {
        if (RootNodeProvider is null)
            throw new InvalidOperationException(nameof(RootNodeProvider));

        return new DictionaryFsDriveInfo(RootNodeProvider, new PSDriveInfo(
           name: drive.Name,
           provider: drive.Provider,
           root: $@"{drive.Name}:\",
           description: drive.Description,
           credential: drive.Credential));
    }
}