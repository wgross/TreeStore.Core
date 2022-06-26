using System.Collections.Generic;
using TreeStore.DictionaryFS.Nodes;

namespace TreeStore.DictionaryFS.Test;

public abstract class ItemCmdletProviderTestBase : PowerShellTestBase
{
    /// <summary>
    /// Arranges a dictionary file system using the given data as root nodes payload.
    /// </summary>
    public IDictionary<string, object?> ArrangeFileSystem(IDictionary<string, object?> data)
    {
        DictionaryFsCmdletProvider.RootNodeProvider = _ => new DictionaryContainerAdapter(data);

        this.ArrangeFileSystem();

        return data;
    }

    /// <summary>
    /// Loads the module from the tests bin directory and creates a drive 'test'.
    /// </summary>
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