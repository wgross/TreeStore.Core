namespace TreeStore.Core.Nodes;

/// <summary>
/// A <see cref="ProviderNode"/> which may have child nodes.
/// </summary>
public record ContainerNode : ProviderNode
{
    public ContainerNode(ICmdletProvider provider, string? name, IServiceProvider underlying)
        : base(provider, name, underlying)
    { }

    /// <summary>
    /// Fetches the child node by name <paramref name="childName"/> case insensitive. Returns false if a child
    /// can't found.
    /// </summary>
    public bool TryGetChildNode(string childName, [NotNullWhen(true)] out ProviderNode? childNode)
    {
        childNode = this.GetChildItems(this.CmdletProvider)
            .FirstOrDefault(n => n.Name.Equals(childName, StringComparison.OrdinalIgnoreCase));
        return childNode is not null;
    }

    #region IGetChildItem

    /// <summary>
    /// Fetches all child nodes from the underlying <see cref="IGetChildItem"/>.
    /// </summary>
    public IEnumerable<ProviderNode> GetChildItems(ICmdletProvider provider)
        => this.InvokeUnderlyingOrDefault<IGetChildItem>(getChildItems => getChildItems.GetChildItems(this.CmdletProvider));

    /// <summary>
    /// Fetches dynamic child item parameters from the underlying <see cref="IGetChildItem"/>.
    /// </summary>
    public object? GetChildItemParameters(string path, bool recurse)
        => this.InvokeUnderlyingOrDefault<IGetChildItem>(getChildItems => getChildItems.GetChildItemParameters(path, recurse));

    /// <summary>
    /// Checks at the underlying <see cref="IGetChildItem"/> i child nodes are available.
    /// </summary>
    public bool HasChildItems(ICmdletProvider provider)
        => this.InvokeUnderlyingOrDefault<IGetChildItem>(getChildItems => getChildItems.HasChildItems(this.CmdletProvider));

    #endregion IGetChildItem

    #region IRemoveChildItem

    /// <summary>
    /// Removes a child node by name <paramref name="childName"/> from te underlying implementation of <see cref="IRemoveChildItem"/>.
    /// </summary>
    public void RemoveChildItem(string childName, bool recurse)
        => this.InvokeUnderlyingOrThrow<IRemoveChildItem>(removeChildItem => removeChildItem.RemoveChildItem(this.CmdletProvider, childName, recurse));

    /// <summary>
    /// Fetches dynamic remove child item parameters from the underlying <see cref="IRemoveChildItem"/>.
    /// </summary>
    public object? RemoveChildItemParameters(string childName, bool recurse)
        => this.InvokeUnderlyingOrDefault<IRemoveChildItem>(newChildItem => newChildItem.RemoveChildItemParameters(childName, recurse));

    #endregion IRemoveChildItem

    #region INewChildItem

    /// <summary>
    /// Creates a new child item with name <paramref name="childName"/> and type <paramref name="itemTypeName"/> using the
    /// underlying implementation of <see cref="INewChildItem"/>.
    /// </summary>
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

    /// <summary>
    /// Fetches the dynamic creation parameter from the underlying implementation of <see cref="INewChildItem"/>.
    /// </summary>
    public object? NewChildItemParameters(string childName, string itemTypeName, object value)
        => this.InvokeUnderlyingOrDefault<INewChildItem>(newChildItem => newChildItem.NewChildItemParameters(childName, itemTypeName, value));

    #endregion INewChildItem

    #region IRenameChildItem

    /// <summary>
    /// Renames the child node <paramref name="childName"/> with the nwe name <paramref name="newName"/> at the underlying
    /// implementation of <see cref="IRenameChildItem"/>
    /// </summary>
    public void RenameChildItem(string childName, string newName)
        => this.InvokeUnderlyingOrThrow<IRenameChildItem>(renameChildItem => renameChildItem.RenameChildItem(this.CmdletProvider, childName, newName));

    /// <summary>
    /// Fetches dynamic rename child item parameters from the underlying <see cref="IRenameChildItem"/>.
    /// </summary>
    public object? RenameChildItemParameters(string childName, string newName)
        => this.InvokeUnderlyingOrDefault<IRenameChildItem>(renameChildItem => renameChildItem.RenameChildItemParameters(childName, newName));

    #endregion IRenameChildItem

    #region ICopyChildItem

    /// <summary>
    /// Copies the child node <paramref name="nodeToCopy"/> to the destination path <paramref name="destination"/>.
    /// </summary>
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

    /// <summary>
    /// Fetches dynamic copy parameters from the underlying implementation of <see cref="ICopyChildItem"/>.
    /// </summary>
    public object? CopyChildItemParameters(string childName, string destination, bool recurse)
        => this.InvokeUnderlyingOrDefault<ICopyChildItem>(copyChildItem => copyChildItem.CopyChildItemParameters(childName, destination, recurse));

    #endregion ICopyChildItem

    #region ICopyChildItemToProvider

    /// <summary>
    /// Delegates the whole copy operation to the provider nodes underlying implementation. The is nothings the provider base can do here.
    /// </summary>
    public void CopyChildItemToProvider(ProviderNode nodeToCopy, PSDriveInfo drive, string destination, bool recurse)
        => this.InvokeUnderlyingOrThrow<ICopyChildItemToProvider>(copyChildItem => copyChildItem.CopyChildItem(this.CmdletProvider, nodeToCopy, drive, destination, recurse));

    #endregion ICopyChildItemToProvider

    #region IMoveChildItem

    /// <summary>
    /// Moves the child node <paramref name="nodeToMove"/> from ist current paranet node <paramref name="parentOfNodeToMove"/> to the destination path <paramref name="destination"/>
    /// using the underlying implementation of <see cref="IMoveChildItem"/>.
    /// </summary>
    public void MoveChildItem(ContainerNode parentOfNodeToMove, ProviderNode nodeToMove, string[] destination)
        => this.InvokeUnderlyingOrThrow<IMoveChildItem>(moveChildItem => moveChildItem.MoveChildItem(this.CmdletProvider, parentOfNodeToMove, nodeToMove, destination));

    /// <summary>
    /// Fetches dynamic move child item parameters from the underlying <see cref="IMoveChildItem"/>.
    /// </summary>
    public object? MoveChildItemParameters(string name, string destination)
        => this.InvokeUnderlyingOrDefault<IMoveChildItem>(moveChildItem => moveChildItem.MoveChildItemParameters(name, destination));

    #endregion IMoveChildItem

    #region ISetChildItemContent

    /// <summary>
    /// Retrieves a content writer for child <paramref name="childName"/> from the underlying implementation of <see cref="ISetChildItemContent"/>.
    /// </summary>
    public IContentWriter? GetChildItemContentWriter(string childName)
        => this.InvokeUnderlyingOrThrow<ISetChildItemContent>(setChildItemContent => setChildItemContent.GetChildItemContentWriter(this.CmdletProvider, childName));

    /// <summary>
    /// Fetches dynamic parameters from the underlying implementation of <see cref="ISetChildItemContent"/>
    /// </summary>
    /// <param name="childName"></param>
    /// <returns></returns>
    public object? SetChildItemContentParameters(string childName)
        => this.InvokeUnderlyingOrDefault<ISetChildItemContent>(setChildTemContent => setChildTemContent.SetChildItemContentParameters(childName));

    #endregion ISetChildItemContent
}