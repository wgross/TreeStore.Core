using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public record ContainerNode : ProviderNode
    {
        public ContainerNode(string? name, IServiceProvider underlying)
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

        public bool TryGetChildNode(string childName, [NotNullWhen(true)] out ProviderNode? childNode)
        {
            childNode = this.GetChildItems().FirstOrDefault(n => n.Name.Equals(childName, StringComparison.OrdinalIgnoreCase));
            return (childNode is not null);
        }

        #region IGetChildItems

        public IEnumerable<ProviderNode> GetChildItems()
            => this.InvokeUnderlyingOrDefault<IGetChildItems>(getChildItems => getChildItems.GetChildItems());

        public object? GetChildItemParameters(string path, bool recurse)
            => this.InvokeUnderlyingOrDefault<IGetChildItems>(getChildItems => getChildItems.GetChildItemParameters(path, recurse));

        public bool HasChildItems()
            => this.InvokeUnderlyingOrDefault<IGetChildItems>(getChildItems => getChildItems.HasChildItems());

        #endregion IGetChildItems

        #region IRemoveChildItem

        ///<inheritdoc/>
        public void RemoveChildItem(string childName, bool recurse)
            => this.InvokeUnderlyingOrThrow<IRemoveChildItem>(removeChildItem => removeChildItem.RemoveChildItem(childName, recurse));

        ///<inheritdoc/>
        public object? RemoveChildItemParameters(string childName, bool recurse)
            => this.InvokeUnderlyingOrDefault<IRemoveChildItem>(newChildItem => newChildItem.RemoveChildItemParameters(childName, recurse));

        #endregion IRemoveChildItem

        #region INewChildItem

        ///<inheritdoc/>
        public ProviderNode? NewChildItem(string childName, string? itemTypeName, object? value)
            => this.InvokeUnderlyingOrThrow<INewChildItem>(newChildItem => newChildItem.NewChildItem(childName, itemTypeName, value));

        ///<inheritdoc/>
        public object? NewChildItemParameters(string childName, string itemTypeName, object value)
            => this.InvokeUnderlyingOrDefault<INewChildItem>(newChildItem => newChildItem.NewChildItemParameters(childName, itemTypeName, value));

        #endregion INewChildItem

        #region IRenameChildItem

        ///<inheritdoc/>
        public void RenameChildItem(string childName, string newName)
            => this.InvokeUnderlyingOrThrow<IRenameChildItem>(renameChildItem => renameChildItem.RenameChildItem(childName, newName));

        /// <inheritdoc/>
        public object? RenameChildItemParameters(string childName, string newName)
            => this.InvokeUnderlyingOrDefault<IRenameChildItem>(renameChildItem => renameChildItem.RenameChildItemParameters(childName, newName));

        #endregion IRenameChildItem

        #region ICopyChildItemRecursive

        public void CopyChildItem(ProviderNode nodeToCopy, string[] destination, bool recurse)
        {
            if (recurse)
            {
                if (this.TryGetUnderlyingService<ICopyChildItemRecursive>(out var copyChildItemRecursive))
                {
                    // the underlying handles the operation istself.

                    copyChildItemRecursive.CopyChildItemRecursive(nodeToCopy, destination);
                }
                else if (this.TryGetUnderlyingService<ICopyChildItem>(out var copyChildItem) && nodeToCopy is ContainerNode containerToCopy)
                {
                    // the underlying can only handle coping without recursion.
                    // copy the source root, than invoke 'CopyChildItem(recurse:true)' on the child nodes.

                    var copiedNode = copyChildItem.CopyChildItem(containerToCopy, destination);

                    if (copiedNode is ContainerNode copiedContainerNode)
                    {
                        // copy the sources roots children

                        foreach (var containerToCopyChild in containerToCopy.GetChildItems())
                        {
                            copiedContainerNode.CopyChildItem(containerToCopyChild, new[] { containerToCopyChild.Name }, recurse);
                        }
                    }
                    else
                    {
                        // the child isn't a container. use single copy operation than w/o recursion.

                        this.GetUnderlyingServiceOrThrow<ICopyChildItem>(out copyChildItem);

                        copyChildItem.CopyChildItem(nodeToCopy, destination);
                    }
                }
            }
            else
            {
                // the copy isn't recursive. Just call the simple copy operation at the source root.
                this.GetUnderlyingServiceOrThrow<ICopyChildItem>(out var copyChildItem);

                copyChildItem.CopyChildItem(nodeToCopy, destination);
            }
        }

        public object? CopyChildItemParameters(string childName, string destination, bool recurse)
            => this.InvokeUnderlyingOrDefault<ICopyChildItem>(copyChildItem => copyChildItem.CopyChildItemParameters(childName, destination, recurse));

        #endregion ICopyChildItemRecursive

        #region IMoveChildItem

        public void MoveChildItem(ContainerNode parentOfNode, ProviderNode nodeToMove, string[] destination)
            => this.InvokeUnderlyingOrThrow<IMoveChildItem>(moveChildItem => moveChildItem.MoveChildItem(parentOfNode, nodeToMove, destination));

        public object? MoveChildItemParameter(string name, string destination)
            => this.InvokeUnderlyingOrDefault<IMoveChildItem>(moveChildItem => moveChildItem.MoveChildItemParameters(name, destination));

        #endregion IMoveChildItem
    }
}