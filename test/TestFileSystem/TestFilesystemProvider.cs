using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Provider;

namespace TestFileSystem
{
    [CmdletProvider(TestFilesystemProvider.Id, ProviderCapabilities.None)]
    public class TestFilesystemProvider : global::PowerShellFilesystemProviderBase.Providers.PowerShellFileSystemProviderBase
    {
        public const string Id = "TestFilesystem";

        public static Func<string, object> RootNodeProvider { get; set; }

        public TestFilesystemProvider()
        {
        }

        protected override Collection<PSDriveInfo> InitializeDefaultDrives()
        {
            return base.InitializeDefaultDrives();
        }

        protected override PSDriveInfo NewDrive(PSDriveInfo drive)
        {
            return new TestFileSystemDriveInfo(RootNodeProvider, new PSDriveInfo(
               name: drive.Name,
               provider: drive.Provider,
               root: drive.Root,
               description: drive.Description,
               credential: drive.Credential));
        }

        protected override object NewDriveDynamicParameters()
        {
            return base.NewDriveDynamicParameters();
        }

        protected override PSDriveInfo RemoveDrive(PSDriveInfo drive)
        {
            return base.RemoveDrive(drive);
        }

        protected override ProviderInfo Start(ProviderInfo providerInfo)
        {
            return base.Start(providerInfo);
        }

        protected override object StartDynamicParameters()
        {
            return base.StartDynamicParameters();
        }

        protected override void Stop()
        {
            base.Stop();
        }

        protected override void StopProcessing()
        {
            base.StopProcessing();
        }
    }
}