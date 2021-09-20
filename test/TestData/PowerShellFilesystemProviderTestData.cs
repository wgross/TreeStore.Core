using Microsoft.Extensions.DependencyInjection;
using Moq;
using PowerShellFilesystemProviderBase.Nodes;
using System;

namespace PowerShellFilesystemProviderBase.Test
{
    public static class TestData
    {
        /// <summary>
        /// Creates a <see cref="IServiceProvider"/> from the service collection configured by the delegates in <paramref name="setup"/>.
        /// </summary>
        public static IServiceProvider ServiceProvider(params Action<ServiceCollection>[] setup)
        {
            var serviceCollection = new ServiceCollection();

            foreach (var s in setup)
                s(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        public static Action<ServiceCollection> With<T>(object capability) where T : class
        {
            if (capability.GetType().IsGenericType && capability.GetType().IsAssignableTo(typeof(Mock)))
                return sp => sp.AddSingleton<T>((T)((Mock)capability).Object);
            else
                return sp => sp.AddSingleton<T>((T)capability);
        }

        public static ContainerNode ContainerNode(string name, params Action<ServiceCollection>[] setup)
        {
            return new ContainerNode(name, ServiceProvider(setup));
        }

        public static LeafNode LeafNode(string name, params Action<ServiceCollection>[] setup)
        {
            return new LeafNode(name, ServiceProvider(setup));
        }

        public static LeafNode ArrangeLeafNode(string name, IServiceProvider sp) => new(name, sp);
    }
}