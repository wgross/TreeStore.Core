using System;
using System.Management.Automation;

namespace TreeStore.Core.Providers;

/// <summary>
/// Base class for a <see cref="PSDriveInfo"/> implementation used by the TreeStore base provider.
/// It extends <sse cref="PSDriveInfo"/> with a method to create the root nodes <see cref="IServiceProvider"/>.
/// </summary>
public abstract class TreeStoreDriveInfoBase : PSDriveInfo
{
    protected TreeStoreDriveInfoBase(PSDriveInfo driveInfo)
        : base(driveInfo)
    {
    }

    /// <summary>
    /// returns the <see cref="IServiceProvider"/> instance representing the root node.
    /// </summary>
    protected internal abstract IServiceProvider GetRootNodeProvider();
}