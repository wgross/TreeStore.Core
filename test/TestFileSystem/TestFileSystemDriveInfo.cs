using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Management.Automation;

namespace TestFileSystem
{
    public class TestFileSystemDriveInfo : PowerShellFilesystemProviderBase.Providers.PowershellFileSystemDriveInfo
    {
        public TestFileSystemDriveInfo(Func<string, object> rootNodeProvider, PSDriveInfo driveInfo)
            : base(driveInfo, rootNodeProvider)
        {
        }
    }
}