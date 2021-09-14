using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public abstract class ProviderNode
    {
        private readonly string name;
        private readonly object underlying;

        protected ProviderNode(string? name, object? underlying)
        {
            if (name is null) throw new ArgumentNullException(nameof(name));
            if (underlying is null) throw new ArgumentNullException(nameof(underlying));

            this.name = name;
            this.underlying = underlying;
        }

        public object Underlying => this.underlying;

        public string Name => this.name;

        #region Delegate to Underlying or ..

        protected void InvokeUnderlyingOrThrow<T>(Action<T> invoke) where T : class
        {
            if (this.Underlying is T t)
            {
                invoke(t);
            }
            else throw this.CapabilityNotSupported<T>();
        }

        protected ProviderNode? InvokeUnderlyingOrThrow<T>(Func<T, ProviderNode?> invoke) where T : class
        {
            return this.Underlying switch
            {
                T t => invoke(t),
                _ => throw this.CapabilityNotSupported<T>()
            };
        }

        protected IEnumerable<ProviderNode> InvokeUnderlyingOrDefault<T>(Func<T, IEnumerable<ProviderNode>> invoke) where T : class
        {
            return this.Underlying switch
            {
                T t => invoke(t),
                _ => Enumerable.Empty<ProviderNode>()
            };
        }

        protected object? InvokeUnderlyingOrDefault<T>(Func<T, object?> invoke) where T : class
        {
            return this.Underlying switch
            {
                T t => invoke(t),
                _ => null
            };
        }

        protected bool InvokeUnderlyingOrDefault<T>(Func<T, bool> invoke, bool defaultValue = false) where T : class
        {
            return this.Underlying switch
            {
                T t => invoke(t),
                _ => defaultValue
            };
        }

        protected Exception CapabilityNotSupported<T>()
            => new PSNotSupportedException($"Node(name='{this.Name}') doesn't provide an implementation of capability '{typeof(T).Name}'.");

        #endregion Delegate to Underlying or ..

        #region IGetItem

        /// <inheritdoc/>
        public object? GetItemParameters()
            => this.InvokeUnderlyingOrDefault<IGetItem>(getItem => getItem.GetItemParameters());

        /// <inheritdoc/>
        public PSObject? GetItem()
        {
            PSObject? pso = default;
            if (this.Underlying is IGetItem getItem)
            {
                pso = getItem.GetItem();
            }
            else
            {
                pso = PSObject.AsPSObject(this.underlying);
            }

            if (pso is null) return null;

            pso.Properties.Add(new PSNoteProperty("PSChildName", this.Name));
            return pso;
        }

        #endregion IGetItem

        #region ISetItem

        /// <inheritdoc/>
        public void SetItem(object value)
            => this.InvokeUnderlyingOrThrow<ISetItem>(setItem => setItem.SetItem(value));

        /// <inheritdoc/>
        public object? SetItemParameters()
           => this.InvokeUnderlyingOrDefault<ISetItem>(setItem => setItem.SetItemParameters());

        #endregion ISetItem

        #region IClearItem

        /// <inheritdoc/>
        public void ClearItem()
            => this.InvokeUnderlyingOrThrow<IClearItem>(clearItem => clearItem.ClearItem());

        /// <inheritdoc/>
        public object? ClearItemParameters()
            => this.InvokeUnderlyingOrDefault<IClearItem>(clearItem => clearItem.ClearItemParameters());

        #endregion IClearItem

        #region IItemExists

        /// <inheritdoc/>
        public bool ItemExists()
            => this.InvokeUnderlyingOrDefault<IItemExists>(itemExists => itemExists.ItemExists(), defaultValue: true);

        /// <inheritdoc/>
        public object? ItemExistsParameters()
            => this.InvokeUnderlyingOrDefault<IItemExists>(itemExists => itemExists.ItemExistsParameters());

        #endregion IItemExists

        #region IInvokeItem

        /// <inheritdoc/>
        public void InvokeItem()
            => this.InvokeUnderlyingOrThrow<IInvokeItem>(invokeItem => invokeItem.InvokeItem());

        /// <inheritdoc/>
        public object? InvokeItemParameters()
            => this.InvokeUnderlyingOrDefault<IInvokeItem>(invokeItem => invokeItem.InvokeItemParameters());

        #endregion IInvokeItem

        #region IClearItemProperty

        public void ClearItemProperty(IEnumerable<string> name)
            => this.InvokeUnderlyingOrThrow<IClearItemProperty>(clearItemProperty => clearItemProperty.ClearItemProperty(name));

        public object? ClearItemPropertyParameters(IEnumerable<string> name)
            => this.InvokeUnderlyingOrDefault<IClearItemProperty>(clearItemProperty => clearItemProperty.ClearItemPropertyParameters(name));

        #endregion IClearItemProperty

        #region IGetItemProperty

        public PSObject? GetItemProperty(IEnumerable<string> providerSpecificPickList)
        {
            if (this.Underlying is IGetItemProperty getItemProperty)
            {
                return getItemProperty.GetItemProperty(providerSpecificPickList);
            }
            else
            {
                var psObject = this.GetItem();
                if (providerSpecificPickList is null || !providerSpecificPickList.Any())
                    return psObject;

                if (psObject is not null)
                {
                    var partialPsObject = new PSObject();
                    foreach (var property in providerSpecificPickList)
                    {
                        var psProperty = psObject.Properties.FirstOrDefault(p => p.Name.Equals(property, StringComparison.OrdinalIgnoreCase));
                        if (psProperty is not null)
                            partialPsObject.Properties.Add(new PSNoteProperty(psProperty.Name, psProperty.Value));
                    }
                    return partialPsObject;
                }
                return psObject;
            }
        }

        public object? GetItemPropertyParameters(IEnumerable<string> providerSpecificPickList)
        {
            if (this.Underlying is IGetItemProperty getItemProperty)
            {
                return getItemProperty.GetItemPropertyParameters(providerSpecificPickList);
            }
            else return null;
        }

        #endregion IGetItemProperty

        #region ISetItemProperty

        public void SetItemProperty(PSObject propertyValue)
            => this.InvokeUnderlyingOrThrow<ISetItemProperty>(setItemProperty => setItemProperty.SetItemProperty(propertyValue));

        public object? SetItemPropertyParameters(PSObject propertyValue)
            => this.InvokeUnderlyingOrDefault<ISetItemProperty>(setItemProperty => setItemProperty.SetItemPropertyParameters(propertyValue));

        #endregion ISetItemProperty

        #region ICopyItemProperty

        public void CopyItemProperty(ProviderNode sourceNode, string sourcePropertyName, string destinationPropertyName)
            => this.InvokeUnderlyingOrThrow<ICopyItemProperty>(copyItemProperty => copyItemProperty.CopyItemProperty(sourceNode, sourcePropertyName, destinationPropertyName));

        public object? CopyItemPropertyParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
            => this.InvokeUnderlyingOrDefault<ICopyItemProperty>(copyItemProperty => copyItemProperty.CopyItemPropertyParameters(sourcePath, sourceProperty, destinationPath, destinationProperty));

        #endregion ICopyItemProperty

        #region IRemoveItemProperty

        public void RemoveItemProperty(string propertyName)
            => this.InvokeUnderlyingOrThrow<IRemoveItemProperty>(remoteItemProperty => remoteItemProperty.RemoveItemProperty(propertyName));

        public object? RemoveItemPropertyParameters(string propertyName)
            => this.InvokeUnderlyingOrDefault<IRemoveItemProperty>(remoteItemProperty => remoteItemProperty.RemoveItemPropertyParameters(propertyName));

        #endregion IRemoveItemProperty

        #region IMoveItemProperty

        public void MoveItemProperty(ProviderNode sourceNode, string sourcePropertyName, string destinationPropertyName)
            => this.InvokeUnderlyingOrThrow<IMoveItemProperty>(moveItemProperty => moveItemProperty.MoveItemProperty(sourceNode, sourcePropertyName, destinationPropertyName));

        public object? MoveItemPropertyParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
           => this.InvokeUnderlyingOrDefault<IMoveItemProperty>(moveItemPropery => moveItemPropery.MoveItemPropertyParameters(sourcePath, sourceProperty, destinationPath, destinationProperty));

        #endregion IMoveItemProperty

        #region INewItemProperty

        public void NewItemProperty(string propertyName, string propertyTypeName, object? value)
            => this.InvokeUnderlyingOrThrow<INewItemProperty>(newItemProperty => newItemProperty.NewItemProperty(propertyName, propertyTypeName, value));

        public object? NewItemPropertyParameter(string propertyName, string propertyTypeName, object? value)
            => this.InvokeUnderlyingOrDefault<INewItemProperty>(newItemProperty => newItemProperty.NewItemPropertyParameters(propertyName, propertyTypeName, value));

        #endregion INewItemProperty

        #region IRenameItemProperty

        public void RenameItemProperty(string propertyName, string newName)
            => this.InvokeUnderlyingOrThrow<IRenameItemProperty>(renameItemProperty => renameItemProperty.RenameItemProperty(propertyName, newName));

        public object? RenameItemPropertyParameters(string propertyName, string newName)
            => this.InvokeUnderlyingOrDefault<IRenameItemProperty>(renameItemProperty => renameItemProperty.RenameItemPropertyParameters(propertyName, newName));

        #endregion IRenameItemProperty
    }
}