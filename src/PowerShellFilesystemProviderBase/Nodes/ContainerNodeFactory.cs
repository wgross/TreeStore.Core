using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public class ContainerNodeFactory
    {
        public static ContainerNode Create<T>(string? name, T underlying)
           where T : IItemContainer
        {
            return new ContainerNode(name, underlying);
        }

        public static ContainerNode Create(string? name, object underlying)
        {
            if (underlying is null) throw new ArgumentNullException(nameof(underlying));

            if (!underlying.GetType().IsDictionaryWithStringKey())
                throw new ArgumentException(message: $"{underlying.GetType()} must implement IDictionary<string,>", nameof(underlying));

            return CreateFromDictionary(name, underlying);
        }

        internal static ContainerNode CreateFromDictionary(string? name, object underlying)
        {
            return new ContainerNode(name, MakeDictionaryAdpater(underlying));
        }

        internal static ContainerNode CreateFromDictionary(object underlying)
        {
            var dictionaryAdapter = MakeDictionaryAdpater(underlying);

            return new ContainerNode(GetName(dictionaryAdapter), dictionaryAdapter);
        }

        private static object MakeDictionaryAdpater(object underlying)
        {
            if (!underlying.GetType().ImplementsGenericDefinition(typeof(IDictionary<,>), out var implementingType))
                throw new InvalidOperationException("type invalid");

            var activated = Activator.CreateInstance(
                type: typeof(DictionaryContainerNode<,>).MakeGenericType(implementingType, implementingType.GetGenericArguments().ElementAt(1)),
                args: underlying);

            if (activated is null)
                throw new InvalidOperationException($"Couldn't activate {underlying}");

            return activated;
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