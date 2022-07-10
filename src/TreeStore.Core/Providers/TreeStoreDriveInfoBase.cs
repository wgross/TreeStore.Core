using System;
using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.Core.Nodes;

namespace TreeStore.Core.Providers
{
    public abstract class TreeStoreDriveInfoBase : PSDriveInfo
    {
        protected TreeStoreDriveInfoBase(PSDriveInfo driveInfo, Func<string, IServiceProvider> rootNodeProvider)
            : base(driveInfo)
        {
            this.rootNodeProvider = rootNodeProvider;
        }

        private readonly Func<string, IServiceProvider> rootNodeProvider;

        public RootNode RootNode(CmdletProvider provider) => new RootNode(provider, this.rootNodeProvider(this.Name));
    }
}