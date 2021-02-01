using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public record DictionaryContainerNode<T, V> : IItemContainer
        where T : IDictionary<string, V>
    {
        public DictionaryContainerNode(T dictionary)
        {
            this.Underlying = (IDictionary<string, V>)dictionary;
        }
        private IDictionary<string, V> Underlying { get; }

        /// <summary>
        /// Fetches the name property if it exists. this is called only unce during
        /// creation of the <see cref="ContainerNode"/>.
        /// </summary>
        public string? Name => this.Underlying.TryGetValue("Name", out var name) ? name?.ToString() : throw new ArgumentNullException(nameof(name));
        
        public (bool exists, ProviderNode? node) TryGetChildNode(string childName)
        {
            if (this.Underlying.TryGetValue(childName, out var childdata))
                return (true, ProviderNodeFactory.Create(childName, childdata));
            return (false, default);
        }
    }
}