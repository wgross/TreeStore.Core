using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public class ProviderNodeFactory
    {
        /// <summary>
        /// Creates the single root node of cmdlets providers model.
        /// </summary>
        /// <param name="underlying"></param>
        /// <returns></returns>
        public static RootNode CreateRoot(object? underlying)
        {
            return new RootNode(underlying);
        }

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

            if (underlying is Dictionary<string, object> dict)
            {
                throw new NotImplementedException();
                //return ContainerNodeFactory.CreateFromDictionary(name, dict);
            }
            else
            {
                return LeafNodeFactory.Create(name, underlying);
            }
        }
    }
}