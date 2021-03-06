using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
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
    public record DictionaryContainerAdapter :
        // ItemCmdletProvider
        IGetItem, ISetItem, IClearItem,
        // ContainerCmdletProvider
        IGetChildItems, IRemoveChildItem, INewChildItem, IRenameChildItem, ICopyChildItem,
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

        public (bool exists, ProviderNode? node) TryGetChildNode(string childName)
        {
            if (this.Underlying.TryGetValue(childName, out var childData))
                if (childData is not null)
                    if (childData is IDictionary<string, object> childDict)
                        return (true, ContainerNodeFactory.CreateFromDictionary(childName, childDict));

            return (false, default);
        }

        public ProviderNode ToProviderNode(string name, IUnderlyingDictionary dict) => new ContainerNode(name, new DictionaryContainerAdapter(dict));

        #region IGetChildItem

        /// <inheritdoc/>
        public bool HasChildItems() => this.GetChildItems().Any();

        /// <inheritdoc/>
        public IEnumerable<ProviderNode> GetChildItems()
        {
            foreach (var item in this.Underlying)
            {
                if (item.Value is not null)
                {
                    if (item.Value is IDictionary<string, object> dict)
                        yield return ContainerNodeFactory.CreateFromDictionary(name: item.Key, dict);
                }
            }
        }

        #endregion IGetChildItem

        #region IGetItem

        /// <inheritdoc/>
        public PSObject? GetItem()
        {
            var pso = new PSObject();
            foreach (var item in this.Underlying)
                pso.Properties.Add(new PSNoteProperty(item.Key, item.Value));
            return pso;
        }

        public void SetItem()
        {
            throw new NotImplementedException();
        }

        #endregion IGetItem

        #region ISetItem

        public void SetItem(object? value)
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
            else throw new InvalidOperationException($"Data of type '{value.GetType()}' can't be assigned");
        }

        #endregion ISetItem

        #region IClearItem

        public void ClearItem() => this.Underlying.Clear();

        #endregion IClearItem

        #region IRemoveChildItem

        /// <inheritdoc/>
        public void RemoveChildItem(string childName)
        {
            this.Underlying.Remove(childName);
        }

        #endregion IRemoveChildItem

        #region INewChildItem

        ///<inheritdoc/>
        public ProviderNode? NewChildItem(string childName, string? itemTypeName, object? value)
        {
            this.Underlying.Add(childName, value);
            return ContainerNodeFactory.Create(childName, value);
        }

        //<inheritdoc/>
        public void RenameChildItem(string childName, string newName)
        {
            if (this.Underlying.TryGetValue(childName, out var childValue))
                if (this.Underlying.TryAdd(newName, childValue))
                    this.Underlying.Remove(childName);
        }

        #endregion INewChildItem

        #region IMoveChildItem

        public void MoveChildItem(ContainerNode parentOfNodeToMove, ProviderNode nodeToMove, string[] destination)
        {
            if (nodeToMove.Underlying is DictionaryContainerAdapter underlyingDict)
            {
                switch (destination.Length)
                {
                    case 0:
                        // put node directly under this node
                        if (this.Underlying.TryAdd(nodeToMove.Name, underlyingDict.Underlying))
                            parentOfNodeToMove.RemoveChildItem(nodeToMove.Name);
                        return;

                    case 1:
                        // put node directly under this node with new name
                        if (this.Underlying.TryAdd(destination[0], underlyingDict.Underlying))
                            parentOfNodeToMove.RemoveChildItem(nodeToMove.Name);
                        return;

                    default:
                        // put node directly under the new node in between
                        var newDict = new Dictionary<string, object?>();
                        if (this.Underlying.TryAdd(destination[0], newDict))
                        {
                            // delegate the move operation recursively
                            var container = new ContainerNode(destination[0], new DictionaryContainerAdapter(newDict));
                            container.MoveChildItem(parentOfNodeToMove, nodeToMove, destination[1..]);
                        }
                        return;
                }
            }
            else throw new NotImplementedException();
        }

        #endregion IMoveChildItem

        #region ICopyChildITem

        public ProviderNode? CopyChildItem(ProviderNode nodeToCopy, string[] destination)
        {
            if (nodeToCopy.Underlying is DictionaryContainerAdapter underlyingDict)
            {
                switch (destination.Length)
                {
                    case 0:
                        // put node directly under this node
                        if (this.Underlying.TryAdd(nodeToCopy.Name, underlyingDict.Underlying.CloneShallow()))
                            return ToProviderNode(nodeToCopy.Name, (IUnderlyingDictionary)this.Underlying[nodeToCopy.Name]!);
                        return null;

                    case 1:
                        // put node directly under this node with new name
                        if (this.Underlying.TryAdd(destination[0], underlyingDict.Underlying.CloneShallow()))
                            return ToProviderNode(destination[0], (IUnderlyingDictionary)this.Underlying[destination[0]]!);
                        return null;

                    default:
                        // put node directly under the new node in between
                        var newDict = new Dictionary<string, object?>();
                        if (this.Underlying.TryAdd(destination[0], newDict))
                        {
                            // delegate the copy operation recursively
                            var container = new DictionaryContainerAdapter(newDict);
                            return container.CopyChildItem(nodeToCopy, destination[1..]);
                        }
                        return null;
                }
            }
            return null;
        }

        #endregion ICopyChildITem

        #region IClearItemProperty

        public void ClearItemProperty(IEnumerable<string> name)
        {
            foreach (var n in name)
            {
                if (this.Underlying.TryGetValue(n, out var value))
                    if (value is IDictionary<string, object>)
                        continue;
                this.Underlying[n] = null;
            }
        }

        #endregion IClearItemProperty

        #region ISetItemProperty

        public void SetItemProperty(PSObject psObject)
        {
            foreach (var property in psObject.Properties)
            {
                if (this.Underlying.TryGetValue(property.Name, out var value))
                    if (value is IDictionary<string, object>)
                        continue;

                this.Underlying[property.Name] = property.Value;
            }
        }

        public void CopyItemProperty(ProviderNode sourceNode, string sourcePropertyName, string destinationPropertyName)
        {
            if (sourceNode.Underlying is DictionaryContainerAdapter underlyingDict)
            {
                this.Underlying[destinationPropertyName] = underlyingDict.Underlying[sourcePropertyName];
            }
        }

        #endregion ISetItemProperty

        #region IRemoveItemProperty

        public void RemoveItemProperty(string propertyName) => this.Underlying.Remove(propertyName);

        #endregion IRemoveItemProperty

        #region IMoveItemProperty

        public void MoveItemProperty(ProviderNode sourceNode, string sourceProperty, string destinationProperty)
        {
            if (sourceNode.Underlying is DictionaryContainerAdapter underlyingDict)
            {
                var sourceValue = underlyingDict.Underlying[sourceProperty];
                if (underlyingDict.Underlying.Remove(sourceProperty))
                    this.Underlying[destinationProperty] = sourceValue;
            }
        }

        #endregion IMoveItemProperty

        #region INewItemProperty

        public void NewItemProperty(string propertyName, string? propertyTypeName, object? value) => this.Underlying.Add(propertyName, value);

        #endregion INewItemProperty

        #region IRenameItemProperty

        public void RenameItemProperty(string sourceProperty, string destinationProperty)
        {
            this.Underlying.Add(destinationProperty, this.Underlying[sourceProperty]);
        }

        #endregion IRenameItemProperty
    }
}