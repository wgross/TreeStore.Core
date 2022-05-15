# TreeStore Capabilities

Capabilities are interface contracts which are invoked by the file system provider nodes to process a PowerShell file system command.
Most capabilities match to a single PowerShell command but some are used at multiple places.

## Path Traversal

Path traversal means that a powershell provider path is mapped to a TreeStore provider node (leaf or container).
It is important that the names of nodes are unique under a parent node.
Traversal of a path always uses the names and no other properties of the nodes payload to identify every node.

### IGetChildItem

This capability is used during path traversal by the TreeStore provider and implements PowerShell command `Get-ChildItem`.

## Implementing ItemCmdletProvider

PowerShells
[ItemCmdletProvider](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.provider.itemcmdletprovider)
is the simplest PowerShell cmdlet provider base classes.
To support it the capabilities below are required:

* IClearItem - Clear the content of a file system item
* IGetItem - Get an item representation to write to the pipe
* IItemExists
* IInvokeItem - Executes a file system item
* ISetItem - Set a filesystem items value

### IGetItem

IGetItem returns a PSObject that is written by the PowerShell provider to the pipe.
Such a PSObject can be created by reflection from any object by calling:

```CSharp
PSObject.AsPSObject(result);
```

Also you may build or extend a PSObject with manually created properties.
> **You will always have to provide this capability to make your provider useful**

### IItemExists

Availability of this capabilitiy is a precondition if several item commandlets that check the existence of an item before they proceed.

This is also what the 'Test-Path' command invokes.

### ISetItem/IClearItem

If a file system item has the semantic of having a 'value' these capability set the value or remove them.
The don't depend on each other and can be implemented independently.

## Implementing ContainerCmdletProvider

PowerShells
[ContainerCmdletProvider](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.provider.containercmdletprovider)
extends the ItemCmdletProvider with the knowledge of Containers/Folders.
This requires the capabilities:

* INewChildItem - create a new child item under a parent node
* IRemoveChildItem - remove a child item from a parent node
* ICopyChildItem - creates a new child item under a parent node as a copy of an another file system node
* ICopyChildItemRecursive - create a copy of a file system tree under a parent node from an existing file system tree
* IMoveChildItem - create a new file system node under a parent and removes this node from ist former parent
* IRenameChildItem - changes the name of a file system node

### ICopyChildItem and ICopyChildItemRecursive

If an recursive copy operation is requested `ICopyChildItemRecursive` may be implemented.
Otherwise TreeStores provider nodes handle the recursion by invoking `ICopyChildItem` one by one.
Also `ICopyChildItem` defines the methods for dynamic parameter definition for both operations.