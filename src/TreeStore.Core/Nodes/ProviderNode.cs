using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Provider;
using TreeStore.Core.Capabilities;

namespace TreeStore.Core.Nodes;

public abstract record ProviderNode
{
    protected ProviderNode(string? name, IServiceProvider underlying)
    {
        ArgumentNullException.ThrowIfNull(name, nameof(name));
        ArgumentNullException.ThrowIfNull(underlying, nameof(underlying));

        this.Name = name;
        this.Underlying = underlying;
    }

    public IServiceProvider Underlying { get; }

    public string Name { get; }

    #region Delegate to Underlying or ..

    protected bool TryGetUnderlyingService<T>([NotNullWhen(true)] out T? service)
    {
        service = this.Underlying.GetService<T>();
        return service is not null;
    }

    protected void GetUnderlyingServiceOrThrow<T>(out T service)
    {
        service = this.Underlying.GetService<T>() ?? throw this.CapabilityNotSupported<T>();
    }

    protected void InvokeUnderlyingOrThrow<T>(Action<T> invoke) where T : class
    {
        if (this.TryGetUnderlyingService<T>(out var service))
            invoke(service);
        else throw this.CapabilityNotSupported<T>();
    }

    protected ProviderNode? InvokeUnderlyingOrThrow<T>(Func<T, ProviderNode?> invoke) where T : class
    {
        this.GetUnderlyingServiceOrThrow<T>(out var service);

        return invoke(service);
    }

    protected IEnumerable<ProviderNode> InvokeUnderlyingOrDefault<T>(Func<T, IEnumerable<ProviderNode>> invoke) where T : class
    {
        return (this.TryGetUnderlyingService<T>(out var service))
            ? invoke(service)
            : Enumerable.Empty<ProviderNode>();
    }

    protected bool TryInvokeUnderlyingOrDefault<T>(Func<T, object?> invoke, out object? result) where T : class
    {
        result = default;

        if (this.TryGetUnderlyingService<T>(out var service))
        {
            result = invoke(service);
            return true;
        }

        return false;
    }

    protected object? InvokeUnderlyingOrDefault<T>(Func<T, object?> invoke) where T : class
    {
        var service = this.Underlying.GetService<T>();
        if (service is null)
            return null;

        return invoke(service);
    }

    protected bool InvokeUnderlyingOrDefault<T>(Func<T, bool> invoke, bool defaultValue = false) where T : class
    {
        var service = this.Underlying.GetService<T>();
        if (service is null)
            return defaultValue;

        return invoke(service);
    }

    protected Exception CapabilityNotSupported<T>()
        => new PSNotSupportedException($"Node(name='{this.Name}') doesn't provide an implementation of capability '{typeof(T).Name}'.");

    #endregion Delegate to Underlying or ..

    #region IGetItem

    /// <inheritdoc/>
    public object? GetItemParameters()
        => this.InvokeUnderlyingOrDefault<IGetItem>(getItem => getItem.GetItemParameters());

    public PSObject GetItem(CmdletProvider provider)
    {
        PSObject pso = (PSObject?)this.InvokeUnderlyingOrDefault<IGetItem>(gi => gi.GetItem(provider)) ?? PSObject.AsPSObject(this.Underlying);

        pso.Properties.Add(new PSNoteProperty("PSChildName", this.Name));
        return pso;
    }

    #endregion IGetItem

    #region ISetItem

    /// <inheritdoc/>
    public void SetItem(CmdletProvider provider, object value)
        => this.InvokeUnderlyingOrThrow<ISetItem>(setItem => setItem.SetItem(provider, value));

    /// <inheritdoc/>
    public object? SetItemParameters()
       => this.InvokeUnderlyingOrDefault<ISetItem>(setItem => setItem.SetItemParameters());

    #endregion ISetItem

    #region IClearItem

    /// <inheritdoc/>
    public void ClearItem(CmdletProvider provider)
        => this.InvokeUnderlyingOrThrow<IClearItem>(clearItem => clearItem.ClearItem(provider));

    /// <inheritdoc/>
    public object? ClearItemParameters()
        => this.InvokeUnderlyingOrDefault<IClearItem>(clearItem => clearItem.ClearItemParameters());

    #endregion IClearItem

    #region IItemExists

    /// <inheritdoc/>
    public bool ItemExists(CmdletProvider provider)
        => this.InvokeUnderlyingOrDefault<IItemExists>(itemExists => itemExists.ItemExists(provider), defaultValue: true);

    /// <inheritdoc/>
    public object? ItemExistsParameters(CmdletProvider provider)
        => this.InvokeUnderlyingOrDefault<IItemExists>(itemExists => itemExists.ItemExistsParameters(provider));

    #endregion IItemExists

    #region IInvokeItem

    /// <inheritdoc/>
    public void InvokeItem(CmdletProvider provider)
        => this.InvokeUnderlyingOrThrow<IInvokeItem>(invokeItem => invokeItem.InvokeItem(provider));

    /// <inheritdoc/>
    public object? InvokeItemParameters(CmdletProvider provider)
        => this.InvokeUnderlyingOrDefault<IInvokeItem>(invokeItem => invokeItem.InvokeItemParameters(provider));

    #endregion IInvokeItem

    #region IClearItemProperty

    public void ClearItemProperty(CmdletProvider provider, IEnumerable<string> name)
        => this.InvokeUnderlyingOrThrow<IClearItemProperty>(clearItemProperty => clearItemProperty.ClearItemProperty(provider, name));

    public object? ClearItemPropertyParameters(IEnumerable<string> name)
        => this.InvokeUnderlyingOrDefault<IClearItemProperty>(clearItemProperty => clearItemProperty.ClearItemPropertyParameters(name));

    #endregion IClearItemProperty

    #region IGetItemProperty

    public PSObject GetItemProperty(CmdletProvider provider, IEnumerable<string>? providerSpecificPickList)
    {
        if (this.TryInvokeUnderlyingOrDefault<IGetItemProperty>(getItemProperty => getItemProperty.GetItemProperty(provider, providerSpecificPickList), out var result))
        {
            return (PSObject)result!;
        }
        else
        {
            var psObject = this.GetItem(provider);
            if (providerSpecificPickList is null || !providerSpecificPickList.Any())
                return psObject;

            var partialPsObject = new PSObject();
            foreach (var property in providerSpecificPickList)
            {
                var psProperty = psObject.Properties.FirstOrDefault(p => p.Name.Equals(property, StringComparison.OrdinalIgnoreCase));
                if (psProperty is not null)
                    partialPsObject.Properties.Add(new PSNoteProperty(psProperty.Name, psProperty.Value));
            }
            return partialPsObject;
        }
    }

    public object? GetItemPropertyParameters(IEnumerable<string>? providerSpecificPickList)
        => this.InvokeUnderlyingOrDefault<IGetItemProperty>(getItemItemProperty => getItemItemProperty.GetItemPropertyParameters(providerSpecificPickList));

    #endregion IGetItemProperty

    #region ISetItemProperty

    public void SetItemProperty(CmdletProvider provider, PSObject propertyValue)
        => this.InvokeUnderlyingOrThrow<ISetItemProperty>(setItemProperty => setItemProperty.SetItemProperty(provider, propertyValue));

    public object? SetItemPropertyParameters(PSObject propertyValue)
        => this.InvokeUnderlyingOrDefault<ISetItemProperty>(setItemProperty => setItemProperty.SetItemPropertyParameters(propertyValue));

    #endregion ISetItemProperty

    #region ICopyItemProperty

    public void CopyItemProperty(CmdletProvider provider, ProviderNode sourceNode, string sourcePropertyName, string destinationPropertyName)
        => this.InvokeUnderlyingOrThrow<ICopyItemProperty>(copyItemProperty => copyItemProperty.CopyItemProperty(provider, sourceNode, sourcePropertyName, destinationPropertyName));

    public object? CopyItemPropertyParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
        => this.InvokeUnderlyingOrDefault<ICopyItemProperty>(copyItemProperty => copyItemProperty.CopyItemPropertyParameters(sourcePath, sourceProperty, destinationPath, destinationProperty));

    #endregion ICopyItemProperty

    #region IRemoveItemProperty

    public void RemoveItemProperty(CmdletProvider provider, string propertyName)
        => this.InvokeUnderlyingOrThrow<IRemoveItemProperty>(remoteItemProperty => remoteItemProperty.RemoveItemProperty(provider, propertyName));

    public object? RemoveItemPropertyParameters(string propertyName)
        => this.InvokeUnderlyingOrDefault<IRemoveItemProperty>(remoteItemProperty => remoteItemProperty.RemoveItemPropertyParameters(propertyName));

    #endregion IRemoveItemProperty

    #region IMoveItemProperty

    public void MoveItemProperty(CmdletProvider provider, ProviderNode sourceNode, string sourcePropertyName, string destinationPropertyName)
        => this.InvokeUnderlyingOrThrow<IMoveItemProperty>(moveItemProperty => moveItemProperty.MoveItemProperty(provider, sourceNode, sourcePropertyName, destinationPropertyName));

    public object? MoveItemPropertyParameters(string sourcePath, string sourceProperty, string destinationPath, string destinationProperty)
       => this.InvokeUnderlyingOrDefault<IMoveItemProperty>(moveItemPropery => moveItemPropery.MoveItemPropertyParameters(sourcePath, sourceProperty, destinationPath, destinationProperty));

    #endregion IMoveItemProperty

    #region INewItemProperty

    public void NewItemProperty(CmdletProvider provider, string propertyName, string propertyTypeName, object? value)
        => this.InvokeUnderlyingOrThrow<INewItemProperty>(newItemProperty => newItemProperty.NewItemProperty(provider, propertyName, propertyTypeName, value));

    public object? NewItemPropertyParameter(string propertyName, string propertyTypeName, object? value)
        => this.InvokeUnderlyingOrDefault<INewItemProperty>(newItemProperty => newItemProperty.NewItemPropertyParameters(propertyName, propertyTypeName, value));

    #endregion INewItemProperty

    #region IRenameItemProperty

    public void RenameItemProperty(CmdletProvider provider, string propertyName, string newName)
        => this.InvokeUnderlyingOrThrow<IRenameItemProperty>(renameItemProperty => renameItemProperty.RenameItemProperty(provider, propertyName, newName));

    public object? RenameItemPropertyParameters(string propertyName, string newName)
        => this.InvokeUnderlyingOrDefault<IRenameItemProperty>(renameItemProperty => renameItemProperty.RenameItemPropertyParameters(propertyName, newName));

    #endregion IRenameItemProperty
}