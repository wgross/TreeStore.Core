using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public class ProviderNodeFactory
    {
        /// <summary>
        /// Creates a provider node with best guesss. Node implementing <see cref="IItemContainer"/>
        /// will be <see cref="ContainerNode"/>, also node implementiong <see cref="IDictionary{TKey, TValue}"/>.
        /// Other node are created as <see cref="LeafNode"/>
        /// a Name will be extracted from a property or dictionary item.
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="name">name of the new node</param>
        /// <returns></returns>
        public static ProviderNode Create(string? name, object? underlying)
        {
            if (underlying is null) throw new ArgumentNullException(nameof(underlying));

            if (underlying is IItemContainer)
            {
                return ContainerNodeFactory.Create(name, underlying);
            }
            else if (underlying.GetType().IsDictionaryWithStringKey())
            {
                return ContainerNodeFactory.CreateFromDictionary(name, underlying);
            }
            else
            {
                return LeafNodeFactory.Create(name, underlying);
            }
        }

        /// <summary>
        /// Creates a provider node with best guesss. Node implementing <see cref="IItemContainer"/>
        /// will be <see cref="ContainerNode"/>, also node implementiong <see cref="IDictionary{TKey, TValue}"/>.
        /// Other node are created as <see cref="LeafNode"/>
        /// a Name will be extracted from a property or dictionary item.
        /// </summary>
        /// <param name="underlying"></param>
        /// <returns></returns>
        public static ProviderNode Create(object underlying)
        {
            if (underlying is null) throw new ArgumentNullException(nameof(underlying));

            if (underlying is IItemContainer)
            {
                return ContainerNodeFactory.Create(GetName(underlying), (IItemContainer)underlying);
            }
            else if (underlying.GetType().IsDictionaryWithStringKey())
            {
                return ContainerNodeFactory.CreateFromDictionary(underlying);
            }
            else
            {
                return LeafNodeFactory.Create(GetName(underlying), underlying);
            }
        }

        private static string? GetName(object underlying)
            => underlying.GetType().GetProperty("Name")?.GetValue(underlying, null) as string;
    }
}