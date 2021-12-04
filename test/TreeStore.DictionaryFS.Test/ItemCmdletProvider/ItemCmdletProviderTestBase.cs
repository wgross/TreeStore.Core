using PowerShellFilesystemProviderBase.Test;
using System.Collections.Generic;
using TreeStore.DictionaryFS.Nodes;

namespace TreeStore.DictionaryFS.Test.ItemCmdletProvider
{
    public abstract class ItemCmdletProviderTestBase : PowershellTestBase
    {
        public void ArrangeFileSystem(IDictionary<string, object?> data)
        {
            DictionaryFilesystemProvider.RootNodeProvider = _ => new DictionaryContainerAdapter(data);

            this.ArrangeFileSystem();
        }

        protected void ArrangeFileSystem()
        {
            this.PowerShell.AddCommand("Import-Module")
                .AddArgument("./TreeStore.DictionaryFS.dll")
                .Invoke();
            this.PowerShell.Commands.Clear();
            this.PowerShell.AddCommand("New-PSDrive")
                .AddParameter("PSProvider", "DictionaryFS")
                .AddParameter("Name", "test")
                .AddParameter("Root", "")
                .Invoke();
            this.PowerShell.Commands.Clear();
        }
    }
}