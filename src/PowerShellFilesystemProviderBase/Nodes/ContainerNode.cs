using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public class ContainerNode : ProviderNode, IGetChildItems, IRemoveChildItem, INewChildItem, IRenameChildItem
    {
        public ContainerNode(string? name, object? underlying)
            : base(name, underlying)
        { }

        #region Reflection Queries

        private IEnumerable<PropertyInfo> GetDictionaryProperties()
        {
            return this.Underlying
                .GetType()
                .GetProperties()
                .Where(pi => IsDictionaryWithStringKey(pi.PropertyType));
        }

        private static bool IsDictionaryWithStringKey(Type type)
        {
            if (!ImplementsGenericDefinition(type, typeof(IDictionary<,>), out var implementingType))
                return false;

            if (implementingType.GetGenericArguments().First().Equals(typeof(string)))
                return true;

            return false;
        }

        /// <summary>
        /// Verifoies iof the <paramref name="type"/> implements the <paramref name="genericInterfaceDefinition"/> and
        /// extracts the type combination in <paramref name="implementingType"/>.
        /// Taken from  from https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Utilities/ReflectionUtils.cs
        /// </summary>
        /// <param name="type"></param>
        /// <param name="genericInterfaceDefinition"></param>
        /// <param name="implementingType"></param>
        /// <returns></returns>
        private static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, [NotNullWhen(true)] out Type? implementingType)
        {
            if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
            {
                implementingType = default;
                return false;
            }

            if (type.IsInterface)
            {
                if (type.IsGenericType)
                {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = default;
            return false;
        }

        #endregion Reflection Queries

        #region Node converter

        private ContainerNode AsContainerNode(string? name, PropertyInfo property)
        {
            return new ContainerNode(name, property.GetValue(this.Underlying, null));
        }

        #endregion Node converter

        #region IItemContainer

        /// <summary>
        /// Override this method to provide children under the root.
        /// </summary>
        /// <remarks>
        /// if not overriode in the payload a child node is always a container node.
        /// The payloads properties are considers as ItemProperies if ther aren't enumerable.
        /// </remarks>
        /// <param name="pathItem"></param>
        /// <returns></returns>
        [Obsolete("For now..")]
        public (bool exists, ProviderNode? node) TryGetChildNode(string childName)
        {
            //if (this.Underlying is IItemContainer itemContainer)
            //{
            //    /// this is also true for <see cref="DictionaryContainerNode{T, V}"/>
            //    /// wrapping a <see cref="IDictionary{TKey, TValue}"/>.
            //    return itemContainer.TryGetChildNode(childName);
            //}

            //var node = this.GetDictionaryProperties()
            //    .Where(p => p.Name.Equals(childName))
            //    .Select(pi => AsContainerNode(childName, pi))
            //    .FirstOrDefault();

            return (false, default);
        }

        #endregion IItemContainer

        #region IGetChildItems

        public IEnumerable<ProviderNode> GetChildItems()
            => this.InvokeUnderlyingOrDefault<IGetChildItems>(getChildItems => getChildItems.GetChildItems());

        public object? GetChildItemParameters()
                => this.InvokeUnderlyingOrDefault<IGetChildItems>(getChildItems => getChildItems.GetChildItemParameters());

        public bool HasChildItems()
            => this.InvokeUnderlyingOrDefault<IGetChildItems>(getChildItems => getChildItems.HasChildItems());

        #endregion IGetChildItems

        #region IRemoveChildItem

        ///<inheritdoc/>
        public void RemoveChildItem(string childName)
            => this.InvokeUnderlyingOrThrow<IRemoveChildItem>(removeChildItem => removeChildItem.RemoveChildItem(childName));

        ///<inheritdoc/>
        public object? RemoveChildItemParameters(string childName)
            => this.InvokeUnderlyingOrDefault<IRemoveChildItem>(newChildItem => newChildItem.RemoveChildItemParameters(childName));

        #endregion IRemoveChildItem

        #region INewChildItem

        ///<inheritdoc/>
        public ProviderNode? NewChildItem(string childName, string itemTypeName, object value)
            => this.InvokeUnderlyingOrThrow<INewChildItem>(newChildItem => newChildItem.NewChildItem(childName, itemTypeName, value));

        ///<inheritdoc/>
        public object? NewChildItemParameters(string childName, string itemTypeName, object value)
            => this.InvokeUnderlyingOrDefault<INewChildItem>(newChildItem => newChildItem.NewChildItemParameters(childName, itemTypeName, value));

        #endregion INewChildItem

        #region IRenameChildItem

        ///<inheritdoc/>
        public void RenameChildItem(string childName, string newName)
            => this.InvokeUnderlyingOrThrow<IRenameChildItem>(renameChildItem => renameChildItem.RenameChildItem(childName, newName));

        public object? RenameChidlItemParameters(string childName, string newName)
        {
            throw new NotImplementedException();
        }

        #endregion IRenameChildItem

    }
}