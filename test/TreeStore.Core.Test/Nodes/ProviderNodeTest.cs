using Moq;
using System;

namespace TreeStore.Core.Test.Nodes
{
    public class ProviderNodeTest : IDisposable
    {
        private readonly MockRepository mocks = new MockRepository(MockBehavior.Strict);

        public void Dispose() => this.mocks.VerifyAll();

        private static object[] ObjectArray(params object[] items)
        {
            return items;
        }
    }
}