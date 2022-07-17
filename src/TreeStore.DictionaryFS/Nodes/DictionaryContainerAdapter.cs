using TreeStore.Core.Capabilities;
using TreeStore.Core.Nodes;
using IUnderlyingDictionary = System.Collections.Generic.IDictionary<string, object?>;

namespace TreeStore.DictionaryFS.Nodes
{
    /// <summary>
    /// Adapt an <see cref="IDictionary{string, TValue}"/> as a container node.
    /// It implements:
    /// <list type="bollet">
    /// <item><see cref="IGetChildItem"/>: converts all dictionary value which qualify as containers to child <see cref="ProviderNode"/></item>
    /// <item><see cref="IGetItem"/>: converts all dictionary value which don't qualify as containers to <see cref="PSNoteProperty"/></item>
    /// <item><see cref="IRemoveChildItem"/>: removes a dictionary item by name</item>
    /// <item><see cref="INewChildItem"/>: add a new child node
    /// <item><see cref="IRenameChildItem"/>: rename an existing child node
    /// </summary>
    /// <typeparam name="TUnderlying"></typeparam>
    /// <typeparam name="V"></typeparam>
    public record DictionaryContainerAdapter : IServiceProvider,
        // ItemCmdletProvider
        IGetItem, ISetItem, IClearItem,
        // ContainerCmdletProvider
        IGetChildItem, IRemoveChildItem, INewChildItem, IRenameChildItem, ICopyChildItem,
        // NavigationCmdletProvider
        IMoveChildItem,
        // ItemPropertyCmdletAdapter
        IClearItemProperty, ISetItemProperty,
        // ItemDynamicPropertyCmdletAdapter
        ICopyItemProperty, IRemoveItemProperty, IMoveItemProperty, INewItemProperty, IRenameItemProperty
    {
        public DictionaryContainerAdapter(IDictionary<string, object?> dictionary)
        {
            this.Underlying = dictionary;
        }

        public IUnderlyingDictionary Underlying { get; }

        public (bool Exists, IServiceProvider NodeServices) TryGetChildNode(string childName)
        {
            if (this.Underlying.TryGetValue(childName, out var childData))
            {
                if (childData is not null)
                {
                    if (childData is IDictionary<string, object> childDict)
                    {
                        return (true, new DictionaryContainerAdapter(childDict!));
                    }
                }
            }

            return (false, default);
        }

        #region IServiceProvider

        public object? GetService(Type serviceType)
        {
            if (this.GetType().IsAssignableTo(serviceType))
                return this;
            else return null;
        }

        #endregion IServiceProvider

        #region IGetChildItem

        /// <inheritdoc/>
        bool IGetChildItem.HasChildItems(CmdletProvider provider) => ((IGetChildItem)this).GetChildItems(provider).Any();

        /// <inheritdoc/>
        IEnumerable<ProviderNode> IGetChildItem.GetChildItems(CmdletProvider provider)
        {
            foreach (var item in this.Underlying)
            {
                if (item.Value is not null)
                {
                    if (item.Value is IDictionary<string, object> dict)
                        yield return new ContainerNode(provider, item.Key, new DictionaryContainerAdapter(dict!));
                }
            }
        }

        #endregion IGetChildItem

        #region IGetItem

        /// <inheritdoc/>
        PSObject IGetItem.GetItem(CmdletProvider provider)
        {
            var pso = new PSObject();
            foreach (var item in this.Underlying)
                pso.Properties.Add(new PSNoteProperty(item.Key, item.Value));
            return pso;
        }

        #endregion IGetItem

        #region ISetItem

        void ISetItem.SetItem(CmdletProvider provider, object? value)
        {
            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }
            else if (value is IDictionary<string, object> dict)
            {
                this.Underlying.Clear();
                foreach (var kv in dict)
                {
                    this.Underlying.Add(kv.Key, kv.Value);
                }
            }
            else
            {
                throw new InvalidOperationException($"Data of type '{value.GetType()}' can't be assigned");
            }
        }

        #endregion ISetItem

        #region IClearItem

        void IClearItem.ClearItem(CmdletProvider provider) => this.Underlying.Clear();

        #endregion IClearItem

        #region IRemoveChildItem

        /// <inheritdoc/>
        void IRemoveChildItem.RemoveChildItem(CmdletProvider provider, string childName, bool recurse)
        {
            // call only if recurse is true?
            if (this.Underlying.TryGetValue(childName, out var value))
                if (value is IDictionary<string, object> dict)
                    this.Underlying.Remove(childName);
        }

        #endregion IRemoveChildItem

        #region INewChildItem

        ///<inheritdoc/>
        NewChildItemResult INewChildItem.NewChildItem(CmdletProvider provider, string childName, string? itemTypeName, object? value)
        {
            if (value is null)
                throw new ArgumentNullException(nameof(value));

            if (value is IDictionary<string, object> dict)
            {
                this.Underlying.Add(childName, value);

                return new(true, childName, new DictionaryContainerAdapter(dict!));
            }

            throw new ArgumentException(message: $"{value.GetType()} must implement IDictionary<string,object>", nameof(value));
        }

        #endregion INewChildItem

        #region IRenameChildItem

        ///<inheritdoc/>
        void IRenameChildItem.RenameChildItem(CmdletProvider provider, string childName, string newName)
        {
            if (this.Underlying.TryGetValue(childName, out var childValue))
                if (this.Underlying.TryAdd(newName, childValue))
                    this.Underlying.Remove(childName);
        }

        #endregion IRenameChildItem

        #region IMoveChildItem

        MoveChildItemResult IMoveChildItem.MoveChildItem(CmdletProvider provider, ContainerNode parentOfNodeToMove, ProviderNode nodeToMove, string[] destination)
        {
            if (nodeToMove.NodeServiceProvider is DictionaryContainerAdapter underlyingDict)
            {
                switch (destination.Length)
                {
                    case 0:
                        // put node directly under this node
                        if (this.Underlying.TryAdd(nodeToMove.Name, underlyingDict.Underlying))
                            parentOfNodeToMove.RemoveChildItem(nodeToMove.Name, recurse: true);
                        return new(true, nodeToMove.Name, underlyingDict);

                    case 1:
                        // put node directly under this node with new name
                        if (this.Underlying.TryAdd(destination[0], underlyingDict.Underlying))
                            parentOfNodeToMove.RemoveChildItem(nodeToMove.Name, recurse: true);
                        return new(true, nodeToMove.Name, underlyingDict);

                    default:
                        // put node directly under the new node in between
                        var newDict = new Dictionary<string, object?>();
                        if (this.Underlying.TryAdd(destination[0], newDict))
                        {
                            // delegate the move operation recursively
                            var container = new ContainerNode(provider, destination[0], new DictionaryContainerAdapter(newDict));
                            container.MoveChildItem(parentOfNodeToMove, nodeToMove, destination[1..]);
                        }
                        return new(true, nodeToMove.Name, new DictionaryContainerAdapter(underlyingDict));
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion IMoveChildItem

        #region ICopyChildITem

        CopyChildItemResult ICopyChildItem.CopyChildItem(CmdletProvider provider, ProviderNode nodeToCopy, string[] destination)
        {
            if (nodeToCopy.NodeServiceProvider is DictionaryContainerAdapter underlyingDict)
            {
                switch (destination.Length)
                {
                    case 0:
                        // put node directly under this node
                        if (this.Underlying.TryAdd(nodeToCopy.Name, underlyingDict.Underlying.CloneShallow()))
                            return new(true, nodeToCopy.Name, new DictionaryContainerAdapter((IUnderlyingDictionary)this.Underlying[nodeToCopy.Name]!));

                        return new(false, null, null);

                    case 1:
                        // put node directly under this node with new name
                        if (this.Underlying.TryAdd(destination[0], underlyingDict.Underlying.CloneShallow()))
                            return new(true, destination[0], new DictionaryContainerAdapter((IUnderlyingDictionary)this.Underlying[destination[0]]!));

                        return new(false, null, null);

                    default:
                        // put node directly under the new node in between
                        var newDict = new Dictionary<string, object?>();
                        if (this.Underlying.TryAdd(destination[0], newDict))
                        {
                            // delegate the copy operation recursively
                            var container = new DictionaryContainerAdapter(newDict);
                            return ((ICopyChildItem)container).CopyChildItem(provider, nodeToCopy, destination[1..]);
                        }

                        return new(false, null, null);
                }
            }
            return new(false, null, null);
        }

        #endregion ICopyChildITem

        #region IClearItemProperty

        void IClearItemProperty.ClearItemProperty(CmdletProvider provider, IEnumerable<string> name)
        {
            foreach (var n in name)
            {
                if (this.Underlying.TryGetValue(n, out var value))
                {
                    if (value is IDictionary<string, object>)
                    {
                        continue;
                    }
                }

                this.Underlying[n] = null;
            }
        }

        #endregion IClearItemProperty

        #region ISetItemProperty

        void ISetItemProperty.SetItemProperty(CmdletProvider provider, PSObject psObject)
        {
            foreach (var property in psObject.Properties)
            {
                if (this.Underlying.TryGetValue(property.Name, out var value))
                {
                    if (value is IDictionary<string, object>)
                    {
                        continue;
                    }
                }

                this.Underlying[property.Name] = property.Value;
            }
        }

        void ICopyItemProperty.CopyItemProperty(CmdletProvider provider, ProviderNode sourceNode, string sourcePropertyName, string destinationPropertyName)
        {
            if (sourceNode.NodeServiceProvider is DictionaryContainerAdapter underlyingDict)
            {
                this.Underlying[destinationPropertyName] = underlyingDict.Underlying[sourcePropertyName];
            }
        }

        #endregion ISetItemProperty

        #region IRemoveItemProperty

        void IRemoveItemProperty.RemoveItemProperty(CmdletProvider provider, string propertyName) => this.Underlying.Remove(propertyName);

        #endregion IRemoveItemProperty

        #region IMoveItemProperty

        void IMoveItemProperty.MoveItemProperty(CmdletProvider provider, ProviderNode sourceNode, string sourceProperty, string destinationProperty)
        {
            if (sourceNode.NodeServiceProvider is DictionaryContainerAdapter underlyingDict)
            {
                var sourceValue = underlyingDict.Underlying[sourceProperty];
                if (underlyingDict.Underlying.Remove(sourceProperty))
                    this.Underlying[destinationProperty] = sourceValue;
            }
        }

        #endregion IMoveItemProperty

        #region INewItemProperty

        void INewItemProperty.NewItemProperty(CmdletProvider provider, string propertyName, string? propertyTypeName, object? value) => this.Underlying.Add(propertyName, value);

        #endregion INewItemProperty

        #region IRenameItemProperty

        void IRenameItemProperty.RenameItemProperty(CmdletProvider provider, string sourceProperty, string destinationProperty)
        {
            this.Underlying.Add(destinationProperty, this.Underlying[sourceProperty]);
        }

        #endregion IRenameItemProperty
    }
}