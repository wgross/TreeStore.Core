using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
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

        private static string? GetName(object underlying)
        {
            try
            {
                return underlying.GetType().GetProperty("Name")?.GetValue(underlying, null) as string;
            }
            catch (TargetInvocationException ex) when (ex.InnerException is ArgumentNullException)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                throw; // never executed. Compiler doesn't know that Throw() throws.
            }
        }
    }
}