using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Providers
{
    public abstract class PowershellFileSystemDriveInfo : PSDriveInfo
    {
        protected PowershellFileSystemDriveInfo(PSDriveInfo driveInfo, Func<string, IServiceProvider> rootNodeProvider)
            : base(driveInfo)
        {
            this.rootNodeProvider = rootNodeProvider;
        }

        private readonly Func<string, IServiceProvider> rootNodeProvider;

        public RootNode RootNode => new RootNode(this.rootNodeProvider(this.Name));
    }
}