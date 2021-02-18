using PowerShellFilesystemProviderBase.Capabilities;
using PowerShellFilesystemProviderBase.Nodes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

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
        IGetChildItems, IRemoveChildItem, INewChildItem, IRenameChildItem,
        // NavigationCmdletProvider
        IMoveChildItem
    {
        public DictionaryContainerAdapter(IDictionary<string, object?> dictionary)
        {
            this.Underlying = dictionary;
        }

        public IDictionary<string, object?> Underlying { get; }

        /// <summary>
        /// Fetches the name property if it exists. this is called only once during
        /// creation of the <see cref="ContainerNode"/>.
        /// </summary>
        public string? Name => this.Underlying.TryGetValue("Name", out var name) ? name?.ToString() : throw new ArgumentNullException(nameof(name));

        public (bool exists, ProviderNode? node) TryGetChildNode(string childName)
        {
            if (this.Underlying.TryGetValue(childName, out var childData))
                if (childData is not null)
                    if (childData is IDictionary<string, object> childDict)
                        return (true, ContainerNodeFactory.CreateFromDictionary(childName, childDict));

            return (false, default);
        }

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

        #region ICopyChildItem

        public void NewChildItemAsCopy(string childName, object copyItemValue)
        {
            if (copyItemValue is IDictionary<string, object> valueDict)
                this.Underlying[childName] = valueDict.ToDictionary(kv => kv.Key, kv => kv.Value);
        }

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

        #endregion ICopyChildItem
    }
}