using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public record DictionaryContainerNode<T, V> : IItemContainer, IGetChildItems, IGetItem
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
            if (this.Underlying.TryGetValue(childName, out var childData))
                if (childData is not null)
                {
                    if (childData is IItemContainer itemContainer)
                        return (true, ContainerNodeFactory.CreateFromIItemContainer(childName, itemContainer));
                    if (childData.GetType().IsDictionaryWithStringKey())
                        return (true, ContainerNodeFactory.CreateFromDictionary(childName, childData));
                }
            return (false, default);
        }

        #region IGetChildItem

        public IEnumerable<ProviderNode> GetChildItems()
        {
            foreach (var item in this.Underlying)
            {
                if (item.Value is not null)
                {
                    if (item.Value is IItemContainer itemContainer)
                        yield return ContainerNodeFactory.CreateFromIItemContainer(name: item.Key, itemContainer);
                    else if (item.Value.GetType().IsDictionaryWithStringKey())
                        yield return ContainerNodeFactory.CreateFromDictionary(name: item.Key, item.Value);
                }
            }
        }

        #endregion IGetChildItem

        #region IGetItem

        public PSObject? GetItem()
        {
            var pso = new PSObject();
            foreach (var item in this.Underlying)
            {
                if (item.Value is not null)
                {
                    if (!(item.Value is IItemContainer))
                        if (!item.Value.GetType().IsDictionaryWithStringKey())
                            pso.Properties.Add(new PSNoteProperty(name: item.Key, item.Value));
                }
                else
                {
                    // TODO: if te value is null it isn't inspscted for its container potential.
                    // It would be mor sphistiocated to inspect the second type parameter pf the ditionary if the
                    // type itself has container genes
                    pso.Properties.Add(new PSNoteProperty(item.Key, null));
                }
            }
            return pso;
        }
    }

    #endregion IGetItem
}