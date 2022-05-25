# DictionaryFS

A sample file system base of TreeStore.Core.

## DictionaryContainerAdapter implements IServiceProvider

The center piece of the sample file system is the adapter class ['DictionaryContainerAdapter'](Nodes/DictionaryContainerAdapter.cs).

The adapter class treats all key-value-pairs which have a value that can be cast to `IDictionary<string,object?>` as a child node.
All other key-value-pairs are properties of the file system item.
The filesystem uses only container nodes, Leaf nodes aren't supported.

The implementation of the `IServiceProvider` interface is simple.
If the requested type is assignable from `this`, the references of the adapter is returned as implementation of the requested service:

```Csharp
public object? GetService(Type serviceType)
{
    if (this.GetType().IsAssignableTo(serviceType))
        return this;
    else return null;
}
```

All [TreeStore capabilities](../TreeStore.Core/Capabilities/readme.md) are implemented as explicit interface implementations.
As an example the implementation of `IGetItem`:

```Csharp
PSObject? IGetItem.GetItem()
{
    var pso = new PSObject();
    foreach (var item in this.Underlying)
        pso.Properties.Add(new PSNoteProperty(item.Key, item.Value));
    return pso;
}
```

The DictionaryFS example doesn't make use of dynamic parameters. 
It relies on the default implementations of the capability interfaces. For `IGetItem` the implementation can be found in [IGetItem.cs](../TreeStore.Core/Capabilities/IGetItem.cs) and it look like this:

```Csharp
object? GetItemParameters() => new RuntimeDefinedParameterDictionary();
```

Alternatively you may just return `null`.

To provide a dynamic parameters for an operation you have to override these methods and return a populated [RuntimeDefinedParameterDictionary](https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.runtimedefinedparameterdictionary). Alternavely a class with annotated properties may be returned:

```csharp
public class Parameters 
{
    [Parameter()]
    public string Parameter1 { get; set; }
}
```

## Custom File System Provider and Drive Info

Two additional required parts of a PowerShell file system are provided with [DictionaryFileSystemProvider](./DictionaryFilesystemProvider.cs) and [DictionaryFileSystemInfo](./DictionaryFileSystemDriveInfo.cs)

```mermaid
sequenceDiagram
    PowerShell ->> PowerShell: New-PSDrive
    PowerShell ->> PSDriveInfo: new
    PSDriveInfo -->> PowerShell: return psDriveInfo
    PowerShell ->> DictionaryFileSystemProvider: NewDrive(psDriveInfo)
    DictionaryFileSystemProvider ->>+ DictionaryFileSystemDriveInfo: create from psDriveInfo

```

The file system provider inherits from the TreeStore provider core and overrides the method which is called when a new file system is mounted.
The received generic `PSDriveInfo` contains the parameters of the powershell `New-PSDrive` command and is used to initialize the custom `DictionaryFileSystemInfo`.

The `DictionaryFileSystemInfo` is the state of the drive and is preserved by the PowerShell process.
It also holds a copy of the root node provider delegate to be able to instantiate the root node of the file system to start a traversal of a requested path.
