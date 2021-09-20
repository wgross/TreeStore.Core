using PowerShellFilesystemProviderBase.Test;
using System.Collections.Generic;
using TestFileSystem;
using TreeStore.DictionaryFS.Nodes;

namespace TreeStore.DictionaryFS.Test.ItemCmdletProvider
{
    public abstract class ItemCmdletProviderTestBase : PowershellTestBase
    {
        private object rootData;

        public void ArrangeFileSystem(IDictionary<string, object?> data)
        {
            this.rootData = data;
            DictionaryFilesystemProvider.RootNodeProvider = _ => new DictionaryContainerAdapter(data);

            this.ArrangeFileSystem();
        }

        protected void ArrangeFileSystem()
        {
            this.PowerShell.AddCommand("Import-Module")
                .AddArgument("./TestFileSystem.dll")
                .Invoke();
            this.PowerShell.Commands.Clear();
            this.PowerShell.AddCommand("New-PSDrive")
                .AddParameter("PSProvider", "TestFileSystem")
                .AddParameter("Name", "test")
                .AddParameter("Root", "")
                .Invoke();
            this.PowerShell.Commands.Clear();
        }
    }
}