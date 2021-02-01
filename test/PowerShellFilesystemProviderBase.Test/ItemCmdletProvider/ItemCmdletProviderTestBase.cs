using TestFileSystem;

namespace PowerShellFilesystemProviderBase.Test.ItemCmdletProvider
{
    public abstract class ItemCmdletProviderTestBase : PowershellTestBase
    {
        private object rootNode;

        public void ArrangeFileSystem(object data)
        {
            this.rootNode = data;
            TestFilesystemProvider.RootNodeProvider = _ => this.rootNode;

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