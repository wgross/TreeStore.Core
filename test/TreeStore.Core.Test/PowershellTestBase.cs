using Moq;
using System;
using System.Management.Automation;

namespace TreeStore.Core.Test
{
    public class PowershellTestBase : IDisposable
    {
        protected MockRepository Mocks { get; } = new MockRepository(MockBehavior.Strict);
        protected PowerShell PowerShell { get; }

        public PowershellTestBase()
        {
            this.PowerShell = PowerShell.Create();
        }

        public void Dispose()
        {
            this.Mocks.VerifyAll();
        }
    }
}