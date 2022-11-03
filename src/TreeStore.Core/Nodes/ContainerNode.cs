namespace TreeStore.Core.Nodes;

/// <summary>
/// A <see cref="ProviderNode"/> which may have child nodes.
/// </summary>
public record ContainerNode : ProviderNode
{
    public ContainerNode(ICmdletProvider provider, string? name, IServiceProvider underlying)
        : base(provider, name, underlying)
    { }

    public bool TryGetChildNode(string childName, [NotNullWhen(true)] out ProviderNode? childNode)
    {
        childNode = this.GetChildItems(this.CmdletProvider).FirstOrDefault(n => n.Name.Equals(childName, StringComparison.OrdinalIgnoreCase));
        return childNode is not null;
    }

    #region IGetChildItem

    ///<inheritdoc/>
    public IEnumerable<ProviderNode> GetChildItems(ICmdletProvider provider)
        => this.InvokeUnderlyingOrDefault<IGetChildItem>(getChildItems => getChildItems.GetChildItems(this.CmdletProvider));

    ///<inheritdoc/>
    public object? GetChildItemParameters(string path, bool recurse)
        => this.InvokeUnderlyingOrDefault<IGetChildItem>(getChildItems => getChildItems.GetChildItemParameters(path, recurse));

    ///<inheritdoc/>
    public bool HasChildItems(ICmdletProvider provider)
        => this.InvokeUnderlyingOrDefault<IGetChildItem>(getChildItems => getChildItems.HasChildItems(this.CmdletProvider));

    #endregion IGetChildItem

    #region IRemoveChildItem

    ///<inheritdoc/>
    public void RemoveChildItem(string childName, bool recurse)
        => this.InvokeUnderlyingOrThrow<IRemoveChildItem>(removeChildItem => removeChildItem.RemoveChildItem(this.CmdletProvider, childName, recurse));

    ///<inheritdoc/>
    public object? RemoveChildItemParameters(string childName, bool recurse)
        => this.InvokeUnderlyingOrDefault<IRemoveChildItem>(newChildItem => newChildItem.RemoveChildItemParameters(childName, recurse));

    #endregion IRemoveChildItem

    #region INewChildItem

    ///<inheritdoc/>
    public ProviderNode? NewChildItem(string childName, string? itemTypeName, object? value)
        => this.InvokeUnderlyingOrThrow<INewChildItem>(newChildItem =>
            {
                var result = newChildItem.NewChildItem(this.CmdletProvider, childName, itemTypeName, value);

                if (!result.Created)
                    return null;

                if (result.NodeServices.IsContainer())
                {
                    return new ContainerNode(this.CmdletProvider, result.Name, result.NodeServices!);
                }
                else
                {
                    return new LeafNode(this.CmdletProvider, result.Name, result.NodeServices!);
                }
            });

    ///<inheritdoc/>
    public object? NewChildItemParameters(string childName, string itemTypeName, object value)
        => this.InvokeUnderlyingOrDefault<INewChildItem>(newChildItem => newChildItem.NewChildItemParameters(childName, itemTypeName, value));

    #endregion INewChildItem

    #region IRenameChildItem

    ///<inheritdoc/>
    public void RenameChildItem(string childName, string newName)
        => this.InvokeUnderlyingOrThrow<IRenameChildItem>(renameChildItem => renameChildItem.RenameChildItem(this.CmdletProvider, childName, newName));

    /// <inheritdoc/>
    public object? RenameChildItemParameters(string childName, string newName)
        => this.InvokeUnderlyingOrDefault<IRenameChildItem>(renameChildItem => renameChildItem.RenameChildItemParameters(childName, newName));

    #endregion IRenameChildItem

    #region ICopyChildItemRecursive

    /// <summary>
    /// The copy request receives the destination node and the reminder of the destination path which could not be
    /// resolved from the file system this.ICmdletProvider. It is up to the node implementation to decide if the remaining path items should be created
    /// on the fly
    /// </summary>
    /// <param name="nodeToCopy">node to copy. May be container of leaf</param>
    /// <param name="destination">path item under this which couldn't be resolved because they don't exist yet</param>
    /// <param name="recurse">Copy must include all child item of the <paramref name="nodeToCopy"/></param>
    public void CopyChildItem(ProviderNode nodeToCopy, string[] destination, bool recurse)
    {
        if (recurse)
        {
            if (this.TryGetUnderlyingService<ICopyChildItemRecursive>(out var copyChildItemRecursive))
            {
                // the underlying handles the operation itself.

                copyChildItemRecursive.CopyChildItemRecursive(this.CmdletProvider, nodeToCopy, destination);
            }
            else if (this.TryGetUnderlyingService<ICopyChildItem>(out var copyChildItem) && nodeToCopy is ContainerNode containerToCopy)
            {
                // the underlying can only handle coping without recursion.
                // copy the source root, than invoke 'CopyChildItem(recurse:true)' on the child nodes.
                var copied = copyChildItem.CopyChildItem(this.CmdletProvider, containerToCopy, destination);
                if (copied is null)
                    this.CmdletProvider.ThrowTerminatingError(new(
                        exception: new InvalidOperationException($"{nameof(ICopyChildItem.CopyChildItem)} failed to copy {nodeToCopy.Name}"),
                        errorId: "copy-1",
                        errorCategory: ErrorCategory.InvalidOperation,
                        targetObject: null));

                // copy the sources roots children
                var copiedContainerNode = new ContainerNode(this.CmdletProvider, copied!.Name, copied.NodeServices!);

                foreach (var containerToCopyChild in containerToCopy.GetChildItems(this.CmdletProvider))
                {
                    copiedContainerNode.CopyChildItem(containerToCopyChild, new[] { containerToCopyChild.Name }, recurse);
                }
            }
            else
            {
                // the child isn't a container. use single copy operation than w/o recursion.

                this.GetUnderlyingServiceOrThrow<ICopyChildItem>(out copyChildItem);

                copyChildItem.CopyChildItem(this.CmdletProvider, nodeToCopy, destination);
            }
        }
        else
        {
            // the copy isn't recursive. Just call the simple copy operation at the source root.
            this.GetUnderlyingServiceOrThrow<ICopyChildItem>(out var copyChildItem);

            copyChildItem.CopyChildItem(this.CmdletProvider, nodeToCopy, destination);
        }
    }

    public object? CopyChildItemParameters(string childName, string destination, bool recurse)
        => this.InvokeUnderlyingOrDefault<ICopyChildItem>(copyChildItem => copyChildItem.CopyChildItemParameters(childName, destination, recurse));

    #endregion ICopyChildItemRecursive

    #region IMoveChildItem

    // TODO: The this.ICmdletProvider gives the original parent to make it easier to remove the connection to the moved node. Necessary?
    public void MoveChildItem(ContainerNode parentOfNodeToMove, ProviderNode nodeToMove, string[] destination)
        => this.InvokeUnderlyingOrThrow<IMoveChildItem>(moveChildItem => moveChildItem.MoveChildItem(this.CmdletProvider, parentOfNodeToMove, nodeToMove, destination));

    public object? MoveChildItemParameters(string name, string destination)
        => this.InvokeUnderlyingOrDefault<IMoveChildItem>(moveChildItem => moveChildItem.MoveChildItemParameters(name, destination));

    #endregion IMoveChildItem

    #region ISetChildItemContent

    public IContentWriter? GetChildItemContentWriter(string childName)
        => this.InvokeUnderlyingOrThrow<ISetChildItemContent>(setChildItemContent => setChildItemContent.GetChildItemContentWriter(this.CmdletProvider, childName));

    public object? SetChildItemContentParameters(string childName)
        => this.InvokeUnderlyingOrDefault<ISetChildItemContent>(setChildTemContent => setChildTemContent.SetChildItemContentParameters(childName));

    #endregion ISetChildItemContent
}