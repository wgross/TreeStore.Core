# TreeStore Capabilities
Capabilities are interface contracts which are invoked by the file system provider nodes to process a PowerShell file system command. Most capabilities match to a single PowerShell command but some are used at multiple places.

## Path Traversal
Path traversal means that a PowerShell provider path is mapped to a TreeStore provider node (leaf or container). It is necessary that the names of nodes are unique under a parent node. Traversal of a path always uses the names and no other properties of the nodes payload to identify every node.

## ICmdletProvider Parameter
To each method implementing a node operation the current instance of [ `CmdletProvider`](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.provider.cmdletprovider) is given. This allows the nodes code to access many of the file system current state and functions like:
- Dynamic Parameters of the current invocation (Property `DynamicParameters`)
- Output messages to the user (methods `WriteDebug`, `WriteVerbose` etc. )
- Read common parameters values (properties `Filter`, `Force`, `Include`)

To improve testability of implementation of the nodes payload the `CmdletProvider` is represented by a interface which mirrors the `CmdletProvider`s public methods and properties. Interface can be mocked easily with libraries like `Moq` and this allows to test node behavior which relies in `CmdletProvider` properties like `Force` or calling methods like `WriteInformation` if necessary.

Set a mock up like this:
```csharp
var cmdletProviderMock = new Mock<ICmdletProvider>();
cmdlerProviderMock.Setup(m => m.Force).Returns(true);
```

### IGetChildItem
Path traversal depends on the implementation of `IGetChildItem`  capability and is also used to implement  `Get-ChildItem` command. Implementation of this interfaces also separates the leaves form the containers. If the capability is available the node is considered a container. If the capability is missing is must be a leaf node. 

There is s special case for invocation of `Get-ChildItem`: `Get-ChildItem -Name` only provides the names of the child items instead of the whole 'item'. PowerShells `ContainerProviderBase` provides a method to implement the case separately. TreeStore maps invocation to `GetChildNames` and `GetChildNamesDynamicParameters` to the implementation of `IGetChildItems` and extracts the name by itself.

A future improvement would be:
- [ ] Provide an `IGetChildNames` capability
	- [ ] and fall back to `IGetChildItem` capability if `IGetChildNames` is missing

## Implementing `ItemCmdletProvider`
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

## Implementing `ContainerCmdletProvider`
PowerShells [`ContainerCmdletProvider`](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.provider.containercmdletprovider) extends the `ItemCmdletProvider` with the knowledge of Containers/Folders.
This requires the capabilities:

* `INewChildItem` - create a new child item under a parent node
* `IRemoveChildItem` - remove a child item from a parent node
* `ICopyChildItem` - creates a new child item under a parent node as a copy of an another file system node
* `ICopyChildItemRecursive` - create a copy of a file system tree under a parent node from an existing file system tree
* `IMoveChildItem` - create a new file system node under a parent and removes this node from ist former parent
* `IRenameChildItem` - changes the name of a file system node

### ICopyChildItem and ICopyChildItemRecursive
A recursive copy operation is handled by the `TreeStoreCmdletProviderBase`  by invoking the `ICopyChildItem` capability for each node of the source.  

If the underlying data structures may handle this process more efficiently itself `ICopyChildItemRecursive` should be implemented to take control of the  process.  `ICopyChildItem` defines the methods for dynamic parameter definition for both operations.

> [!NOTE] Implement `ICopyChildItemRecursive` for optimization or atomicity
> If copying an item is costly (time or IO) this capability may be used to optimize the process in an payload specific way. This might be neccessary as well for concurrently used resources which require locks to avoid race conditions.

### Implementing `NavigationCmdletProvider`
PowerShells [`NavigationCmdletProvider`](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.provider.navigationcmdletprovider) extends the `ContainerCmdleProvider` with the knowledge of hierarchy of Items and Containers. 

This requires the capability: `IMoveChildItem` 

### Implementing `IPropertyCmdletProvider`
The capability `IGetItemProperty` supports PowerShells `Get-ItemProperty`  command. If the capability isn't implemented `TreeStoreCmdletProviderBase` will fall back to its own logic: The result of `IGetItem.GetItem` will projected to a new `PObject` containing only the properties requested by the caller.

This requires the capabilities:
- `ISetItemProperty`
- `IClearItemProperty`
- `IGetItemProperty`

### Implementing `IDynamicPropertyCmdletProvider`
For supporting PowerShell `*-ItemProperty*` commands which modify properties dynamically these capabilities: are required:
- `INewItemProperty`
- `IRemoveItemProperty`
- `ICopyItemProperty`
- `IMoveItemProperty`
- `IRenameItemProperty`

## Implementing `IContentProvider`
Supporting PowerShells `*-Content` commands is optional and works a bit different than the other providers at least for reading and writing content. PowerShell will ask for a reader or writer implementation (`IContentReader`, `IContentWriter`) to interact with. It will ask for 'blocks' of information to read and write. The implementation has to decide what such a 'block' of information is. 

Not all capabilities have to be implemented:
- `IClearItemContent`
- `IGetItemContent`
- `ISetChildItemContent`

As for the reader and writer implementations: It seems that PowerShell only call the `Read(long readcont)` and the `Writer(IList)` method. `Seek(..)` seems to be reserved for the internal file system provider.

### Implementing `ISetChiIdItemContent` at the parent node
It is necessary to implement this capability at the parent node. instead of the actula node the set the content at. This Because the parenet node may create the child node during this process.  