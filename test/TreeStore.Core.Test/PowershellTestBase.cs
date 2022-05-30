using Moq;
using System;
using System.Management.Automation;

namespace TreeStore.Core.Test
{
    public abstract class PowerShellTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);
        protected PowerShell PowerShell { get; }

        public PowerShellTestBase() => this.PowerShell = PowerShell.Create();

        public void Dispose()
        {
            this.PowerShell.Dispose();
            this.Mocks.VerifyAll();
        }
    }
}