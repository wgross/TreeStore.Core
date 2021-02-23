using System;
using System.Collections.Generic;
using TreeStore.DictionaryFS.Nodes;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public class ContainerNodeFactory
    {
        public static ContainerNode Create(string? name, object? underlying)
        {
            if (underlying is null) throw new ArgumentNullException(nameof(underlying));

            if (underlying is IDictionary<string, object> dict)
                return CreateFromDictionary(name, dict);

            throw new ArgumentException(message: $"{underlying.GetType()} must implement IDictionary<string,object>", nameof(underlying));
        }

        /// <summary>
        /// Creates a <see cref="ContainerNode"/> from a <see cref="IDictionary{TKey, TValue}"/> and a name.
        /// the <paramref name="underlying"/> isn't checked again.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="underlying"></param>
        /// <returns></returns>
        internal static ContainerNode CreateFromDictionary(string? name, IDictionary<string, object> underlying)
        {
            return new ContainerNode(name, new DictionaryContainerAdapter(underlying!));
        }
    }
}