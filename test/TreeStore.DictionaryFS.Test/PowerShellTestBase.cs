using System;
using System.Management.Automation;

namespace TreeStore.DictionaryFS.Test;

public abstract class PowerShellTestBase : IDisposable
{
    /// <summary>
    /// The <see cref="PowerShell"/> instance to be used in the test case.
    /// </summary>
    protected PowerShell PowerShell { get; }

    public PowerShellTestBase() => this.PowerShell = PowerShell.Create();

    public void Dispose() => this.PowerShell.Dispose();
}