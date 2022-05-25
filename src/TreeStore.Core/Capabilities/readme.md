# TreeStore Capabilities

Capabilities are interface contracts which are invoked by the file system provider nodes to process a PowerShell file system command.
Most capabilities match to a single PowerShell command but some are used at multiple places.

## Path Traversal

Path traversal means that a PowerShell provider path is mapped to a TreeStore provider node (leaf or container). It is necessary that the names of nodes are unique under a parent node. Traversal of a path always uses the names and no other properties of the nodes payload to identify every node.

### IGetChildItem

Path traversal depends on the implementation of `IGetChildItem`  capability and is also by  `Get-ChildItem`.

## Implementing ItemCmdletProvider

PowerShells [ItemCmdletProvider](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.provider.itemcmdletprovider) is the simplest PowerShell cmdlet provider base classes. To support it completely the capabilities below are required:

* `IClearItem` - Clear the content of a file system item
* `IGetItem `- Get an item representation to write to the pipe
* `IItemExists` - Used by several cmdlets to verify existence of the node.
* `IInvokeItem` - Executes a file system item
* `ISetItem` - Set a filesystem items value

### IGetItem

`IGetItem` returns a [`PSObject`](https://docs.microsoft.com/en-us/powershell/scripting/developer/ets/overview#the-psobject-class) that is written by the PowerShell provider to the pipe. Such a `PSObject` can be created by reflection from any object by calling:

```CSharp
PSObject.AsPSObject(result);
```

Also you may build or extend a `PSObject` with manually created properties. If this capability isn't available the payload it self is used to build a `PSObject` using the method above. 

>[!NOTE] Always implement `IGetItem` for real world scenarios
>Piping the payload to the caller may expose implementation details which should remain private.

### IItemExists

Availability of this capability is a precondition for several item commandlets that check the existence of an item before they proceed. This is also what the 'Test-Path' command invokes.
If TreeStore find the node and the node doesn't implement `IItemExists` true is returned. This should fit most uses cases therefore implement `IItemExists` only if the existence of a file system item depends on more then just the path resolution.

### ISetItem/IClearItem

If a file system item has the semantic of having a 'value' these capability set the value or clear it. They don't depend on each other and can be implemented independently. There is no default behavior for this capabilities. If called and missing in the  payload an error is returned.

## Implementing ContainerCmdletProvider

PowerShells [ContainerCmdletProvider](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.provider.containercmdletprovider) extends the `ItemCmdletProvider` with the knowledge of Containers/Folders.
This requires the capabilities:

* `INewChildItem` - create a new child item under a parent node
* `IRemoveChildItem` - remove a child item from a parent node
* `ICopyChildItem` - creates a new child item under a parent node as a copy of an another file system node
* `ICopyChildItemRecursive` - create a copy of a file system tree under a parent node from an existing file system tree
* `IMoveChildItem` - create a new file system node under a parent and removes this node from ist former parent
* `IRenameChildItem` - changes the name of a file system node

### ICopyChildItem and ICopyChildItemRecursive

If an recursive copy operation is requested `ICopyChildItemRecursive` may be implemented. Otherwise TreeStores provider nodes handle the recursion by invoking `ICopyChildItem` one by one. Also `ICopyChildItem` defines the methods for dynamic parameter definition for both operations.

> [!NOTE] Implement `ICopyChildItemRecursive` for optimization 
> If copying an item is costly (time or IO) this capability may be used to optimize the process in an payload specific way. This might be neccessary as well for concurrently used resources which require locks to avoid race conditions.