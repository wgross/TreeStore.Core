# TreeStore.ProviderCore

Framework to help building PowerShell Command providers.

## Core Concepts

### The Cmdlet Provider

The provider (implemented by [PowerShellFileSystemProviderBase](./src/PowerShellFilesystemProviderBase/Providers/PowershellFileSystemDriveInfo.cs)) derives from PowerShells ```NavigationCmdletProvider``` which allows the use of all item Cmdlets powershell provides for file system access. In addition it implements the ```IDynamicPropertyCmdletProvider``` which enables interaction with dynamic properties (New-,Remove-,Copy- and Move-ItemProperty) and ```IPropertyCmdletProvider``` for non-dynamic item property interaction.

The provider is only meant to be a base class for a custom provider.
The sample implementation of [DictionaryFS](./src/TreeStore.DictionaryFS/readme.md) shows how a file system derives from the provider base and add a custom ```PSDriveInfo``` to hold the drives state.

### Path Traversal and Nodes

The provider base itself doesn't implement the file system operations.
It searches along the given path to find a node that implement the invoked operation.
During a path traversal starting at the root node child nodes are temporarily created.

The TreeStore provider core maintains its own hierarchical data structure made from [provider nodes](src/PowerShellFilesystemProviderBase/Nodes/readme.md).

If a destination node was identified with a path the operations implementation is invoked at the node. Similar to the provider itself the node classes have a baked in default behavior which makes the operation fails if its not backed by the nodes payload.

The nodes payload is where the custom file system behavior is implemented.

### Provider Node Capabilities

Starting with the root node a implementation of a ```IServiceProvider```has to be provided.
If a nodes operation is called by the powershell provider the node will ask the payloads service provider for the required capability interfaces to process the invocation.
If the capability was provided it is called otherwise the node defaults.

The sample file system  ['DictionaryFS'](src/TreeStore.DictionaryFS/readme.md) shows an implementation of the provider node capabilities and a ```IServiceProvider``` using nested 'IDictionary<string,object>' instances.
This file system is also used to write integration tests for the provider logic.
