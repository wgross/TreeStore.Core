using PowerShellFilesystemProviderBase.Capabilities;
using System;
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

        public bool IsContainer => this.Underlying is IItemContainer;

        public string Name => this.name;

        protected Exception CapabilityNotSupported<T>()
            => new PSNotSupportedException($"Node(name='{this.Name}') doesn't provide an implementation of capability '{typeof(T).Name}'.");

        #region IGetItem

        public object? GetItemParameters()
        {
            if (this.Underlying is IGetItem getItem)
            {
                return getItem.GetItemParameters();
            }
            else return new RuntimeDefinedParameterDictionary();
        }

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

        public void SetItem(object value)
        {
            if (this.Underlying is ISetItem setItem)
            {
                setItem.SetItem(value);
            }
            else throw new PSNotSupportedException($"Item '{this.Name}' can't be set");
        }

        public object? SetItemParameters()
        {
            if (this.Underlying is ISetItem setItem)
            {
                return setItem.SetItemParameters();
            }
            else return new RuntimeDefinedParameterDictionary();
        }

        #endregion ISetItem

        #region IClearItem

        public void ClearItem()
        {
            if (this.Underlying is IClearItem clearItem)
            {
                clearItem.ClearItem();
            }
            else throw new PSNotSupportedException($"Item '{this.Name}' can't be cleared");
        }

        public object? ClearItemParameters()
        {
            if (this.Underlying is IClearItem clearItem)
            {
                return clearItem.ClearItemParameters();
            }
            else return new RuntimeDefinedParameterDictionary();
        }

        #endregion IClearItem

        #region IItemExists

        public bool ItemExists()
        {
            if (this.Underlying is IItemExists itemExists)
            {
                return itemExists.ItemExists();
            }
            return true;
        }

        public object? ItemExistsParameters()
        {
            if (this.Underlying is IItemExists itemExists)
            {
                return itemExists.ItemExistsParameters();
            }
            else return new RuntimeDefinedParameterDictionary();
        }

        #endregion IItemExists

        #region IInvokeItem

        public void InvokeItem()
        {
            if (this.Underlying is IInvokeItem invokeItem)
            {
                invokeItem.InvokeItem();
            }
            else throw this.CapabilityNotSupported<IInvokeItem>();
        }

        public object? InvokeItemParameters()
        {
            if (this.Underlying is IInvokeItem invokeItem)
            {
                return invokeItem.InvokeItemParameters();
            }
            else return new RuntimeDefinedParameterDictionary();
        }

        #endregion IInvokeItem
    }
}