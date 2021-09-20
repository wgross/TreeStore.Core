# Nodes of PowershellProviderBase

The Powershell provider base follows a received path to a node instance.

![node types](./nodes.svg)

The nodes types are implemented a immutable records.
Their state must not change and except of the root node the live only as long as the path is traversed.

The responsibility of the node is to process the command which was invoked at the PowerShell in context of the node which was selected by the path which was part of the command invocation.

Each node was initialized with a name and payload from te outside which defines the nodes capabilities.
The name must not change during the lifetime of the node and neither may the semantic of the node change (container or leaf)

The payload must implement the interface [IServiceProvider](https://docs.microsoft.com/en-us/dotnet/api/system.iserviceprovider). The node will ask for an interface type which is required to process the call from the powershell. 
The capability will be one of these:

- IClearItem
- IClearItemProperty
- ICopyChildItem
- ICopyChildItemRecursive
- ICopyItemProperty
- IGetChildItem
- IGetChildItems
- IGetItem
- IGetItemProperty
- IInvokeItem
- IItemContainer
- IItemExists
- IMoveChildItem
- IMoveItemProperty
- INewChildItem
- INewItemProperty
- IRemoveChildItem
- IRemoveItemProperty
- IRenameChildItem
- IRenameItemProperty
- ISetItem
- ISetItemProperty

If the nodes service provider can deliver and instance of these service types it will be called otherwise the node will fall back to its default behavior.