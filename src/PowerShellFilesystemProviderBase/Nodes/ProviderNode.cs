using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public abstract class ProviderNode :
        IGetItem, ISetItem, IClearItem, IItemExists, IInvokeItem

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
    }
}