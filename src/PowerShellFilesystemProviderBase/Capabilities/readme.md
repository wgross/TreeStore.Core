# TreeStore Capabilities

Capabilities are interface contract whichr are invoked by the file system provider nodes to process a PowerShell file system command.
Most capabilities match to a single PowerShell command but some are used at multiple places.

## Path Traversal

Path traversal means that a powershell provider path is mapped to a TreeStore provider node (leaf or container).

### IGetChildItems

This capability is used during path traversal by the TreeStore provider and doesn't match to any of the PowerShell commands.
A container provider node is asked for a child node having a specified name.

## ItemCmdletProvider

PowerShells
[ItemCmdletProvider](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.provider.itemcmdletprovider)
is the simplest PowerShell cmdlet provider base classes.
Ths requires the capabilities:

* IClearItem
* IGetItem
* IItemExists
* IInvokeItem
* ISetItem

### IGetItem

IGetItem returns a PSObject that is written by the PowerShell provider to the pipe.
Such a PSObject can be created by refelection from any object by calling:

```CSharp
PSObject.AsPSObject(result);
```

Alo you may build or extend a PSObject with manually created properties.
You will always have to provide this capability to make your provider useful.

### ItemExists

Availability of this capabilitiy is a precondition if several item commandlets that check the existence of an item before they proceed.

This also what the 'Test-Path' command invokes.

## ContainerCmdletProvider

PowerShells
[ContainerCmdletProvider](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.provider.containercmdletprovider)
extends the ItemCmdletProvider with the knowledge of Containers/Folders.
This requires the capabilities:

* INewChildItem
* IRemoveChildItem
* ICopyChildItem
* ICopyChildItemRecursive
* IMoveChildItem
* IRenameChildItem

### ICopyChildItem and ICopyChildItemRecursive

If an recursive copy operation is handled by the adapter ```ICopyChildItemRecursive``` may be implemented.
Otherwise TreeStores provider nodes handle the recursion.
Event if a recursive copy is done by the adapter the ```ICopyChildItem``` must be implemented for non-recursive copy operations.
Also ```ICopyChildItem``` defines the methods for dynamic parameter definition.