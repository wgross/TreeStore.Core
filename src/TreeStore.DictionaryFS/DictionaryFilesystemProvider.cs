using System;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace TreeStore.DictionaryFS
{
    [CmdletProvider(DictionaryFilesystemProvider.Id, ProviderCapabilities.None)]
    public class DictionaryFilesystemProvider : global::TreeStore.Core.Providers.TreeStoreCmdletProviderBase
    {
        public const string Id = "DictionaryFS";

        /// <summary>
        /// Creates the root node. The inout string ois the drive name.
        /// </summary>
        public static Func<string, IServiceProvider>? RootNodeProvider { get; set; }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            if (RootNodeProvider is null)
                throw new InvalidOperationException(nameof(RootNodeProvider));

            return new DictionaryFileSystemDriveInfo(RootNodeProvider, new PSDriveInfo(
               name: drive.Name,
               provider: drive.Provider,
               root: $@"{drive.Name}:\",
               description: drive.Description,
               credential: drive.Credential));
        }
    }
}