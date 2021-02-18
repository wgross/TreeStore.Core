using PowerShellFilesystemProviderBase;
using PowerShellFilesystemProviderBase.Test;
using System.Linq;
using System.Management.Automation;
using Xunit;

namespace TreeStore.DictionaryFS.Test.DriveCmdletProvider
{
    public class DriveCmdletProviderTest : PowershellTestBase
    {
        public DriveCmdletProviderTest()
        {
            this.PowerShell.AddCommand("Import-Module").AddArgument("./TestFileSystem.dll").Invoke();
            this.PowerShell.Commands.Clear();
        }

        [Fact]
        public void Powershell_creates_new_drive()
        {
            // ACT
            var result = this.PowerShell.AddCommand("New-PSDrive")
              .AddParameter("PSProvider", "TestFileSystem")
              .AddParameter("Name", "test")
              .AddParameter("Root", "")
              .Invoke()
              .ToArray();

            // ASSERT
            Assert.False(this.PowerShell.HadErrors);

            var psdriveInfo = result.Single().Unwrap<PSDriveInfo>();

            Assert.Equal("test", psdriveInfo.Name);
            Assert.Equal("", psdriveInfo.Root);
        }
    }
}