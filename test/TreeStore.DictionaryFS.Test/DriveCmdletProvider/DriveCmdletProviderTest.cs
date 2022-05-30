using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using TreeStore.Core;
using TreeStore.DictionaryFS.Nodes;
using Xunit;

namespace TreeStore.DictionaryFS.Test.DriveCmdletProvider;

[Collection(nameof(PowerShell))]
public class DriveCmdletProviderTest : PowerShellTestBase
{
    public DriveCmdletProviderTest()
    {
        DictionaryFsCmdletProvider.RootNodeProvider = _ => new DictionaryContainerAdapter(new Dictionary<string, object?>());

        this.PowerShell.AddCommand("Import-Module").AddArgument("./TreeStore.DictionaryFS.dll").Invoke();
        this.PowerShell.Commands.Clear();
    }

    [Fact]
    public void Powershell_creates_new_drive()
    {
        // ACT
        var result = this.PowerShell.AddCommand("New-PSDrive")
          .AddParameter("PSProvider", "DictionaryFS")
          .AddParameter("Name", "test")
          .AddParameter("Root", "")
          .Invoke()
          .ToArray();

        // ASSERT
        Assert.False(this.PowerShell.HadErrors);

        var psdriveInfo = result.Single().Unwrap<PSDriveInfo>();

        Assert.Equal("test", psdriveInfo.Name);
        Assert.Equal("test:\\", psdriveInfo.Root);
    }
}