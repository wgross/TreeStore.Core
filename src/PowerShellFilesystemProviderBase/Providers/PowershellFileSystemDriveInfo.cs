using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Providers
{
    public abstract class PowershellFileSystemDriveInfo : PSDriveInfo
    {
        protected PowershellFileSystemDriveInfo(PSDriveInfo driveInfo, Func<string, object> rootNodeProvider)
            : base(driveInfo)
        {
            this.rootNodeProvider = rootNodeProvider;
        }

        private readonly Func<string, object> rootNodeProvider;

        public ProviderNode RootNode => ProviderNodeFactory.Create(string.Empty, this.rootNodeProvider(this.Name));

    }
}