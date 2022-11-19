using TreeStore.DictionaryFS.Nodes;

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
    /// Creates a new drive from the given creation parameters in <paramref name="drive"/>.
    /// </summary>
    protected override PSDriveInfo NewDrive(PSDriveInfo drive)
    {
        if (this.DynamicParameters is NewDriveParameters newDriveItem)
        {
            return new DictionaryFsDriveInfo(new DictionaryContainerAdapter(newDriveItem.FromDictionary), new PSDriveInfo(
               name: drive.Name,
               provider: drive.Provider,
               root: $@"{drive.Name}:\",
               description: drive.Description,
               credential: drive.Credential));
        }
        else
        {
            // create an empty provider
            return new DictionaryFsDriveInfo(new DictionaryContainerAdapter(new Dictionary<string, object?>()), new PSDriveInfo(
               name: drive.Name,
               provider: drive.Provider,
               root: $@"{drive.Name}:\",
               description: drive.Description,
               credential: drive.Credential));
        }
    }

    protected override object NewDriveDynamicParameters()
    {
        return new NewDriveParameters();
    }
}

public sealed class NewDriveParameters
{
    [Parameter()]
    public IDictionary<string, object?> FromDictionary { get; set; } = new Dictionary<string, object?>();
}