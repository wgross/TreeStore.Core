using PowerShellFilesystemProviderBase.Capabilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace PowerShellFilesystemProviderBase.Nodes
{
    public record ContainerNode : ProviderNode
    {
        public ContainerNode(string? name, IServiceProvider underlying)
            : base(name, underlying)
        { }

        public bool TryGetChildNode(string childName, [NotNullWhen(true)] out ProviderNode? childNode)
        {
            childNode = this.GetChildItems().FirstOrDefault(n => n.Name.Equals(childName, StringComparison.OrdinalIgnoreCase));
            return childNode is not null;
        }

        #region IGetChildItems

        public IEnumerable<ProviderNode> GetChildItems()
            => this.InvokeUnderlyingOrDefault<IGetChildItems>(getChildItems => getChildItems.GetChildItems());

        public object? GetChildItemParameters(string path, bool recurse)
            => this.InvokeUnderlyingOrDefault<IGetChildItems>(getChildItems => getChildItems.GetChildItemParameters(path, recurse));

        public bool HasChildItems()
            => this.InvokeUnderlyingOrDefault<IGetChildItems>(getChildItems => getChildItems.HasChildItems());

        #endregion IGetChildItems

        #region IRemoveChildItem

        ///<inheritdoc/>
        public void RemoveChildItem(string childName, bool recurse)
            => this.InvokeUnderlyingOrThrow<IRemoveChildItem>(removeChildItem => removeChildItem.RemoveChildItem(childName, recurse));

        ///<inheritdoc/>
        public object? RemoveChildItemParameters(string childName, bool recurse)
            => this.InvokeUnderlyingOrDefault<IRemoveChildItem>(newChildItem => newChildItem.RemoveChildItemParameters(childName, recurse));

        #endregion IRemoveChildItem

        #region INewChildItem

        ///<inheritdoc/>
        public ProviderNode? NewChildItem(string childName, string? itemTypeName, object? value)
            => this.InvokeUnderlyingOrThrow<INewChildItem>(newChildItem => newChildItem.NewChildItem(childName, itemTypeName, value));

        ///<inheritdoc/>
        public object? NewChildItemParameters(string childName, string itemTypeName, object value)
            => this.InvokeUnderlyingOrDefault<INewChildItem>(newChildItem => newChildItem.NewChildItemParameters(childName, itemTypeName, value));

        #endregion INewChildItem

        #region IRenameChildItem

        ///<inheritdoc/>
        public void RenameChildItem(string childName, string newName)
            => this.InvokeUnderlyingOrThrow<IRenameChildItem>(renameChildItem => renameChildItem.RenameChildItem(childName, newName));

        /// <inheritdoc/>
        public object? RenameChildItemParameters(string childName, string newName)
            => this.InvokeUnderlyingOrDefault<IRenameChildItem>(renameChildItem => renameChildItem.RenameChildItemParameters(childName, newName));

        #endregion IRenameChildItem

        #region ICopyChildItemRecursive

        /// <summary>
        /// The copy request receives the destination node and the reminder of the destination path which could not be
        /// resolved from the file system provider. It is up to the node implementation to decide if the remaining path items should be created
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

                    copyChildItemRecursive.CopyChildItemRecursive(nodeToCopy, destination);
                }
                else if (this.TryGetUnderlyingService<ICopyChildItem>(out var copyChildItem) && nodeToCopy is ContainerNode containerToCopy)
                {
                    // the underlying can only handle coping without recursion.
                    // copy the source root, than invoke 'CopyChildItem(recurse:true)' on the child nodes.

                    var copiedNode = copyChildItem.CopyChildItem(containerToCopy, destination);

                    if (copiedNode is ContainerNode copiedContainerNode)
                    {
                        // copy the sources roots children

                        foreach (var containerToCopyChild in containerToCopy.GetChildItems())
                        {
                            copiedContainerNode.CopyChildItem(containerToCopyChild, new[] { containerToCopyChild.Name }, recurse);
                        }
                    }
                    else
                    {
                        // the child isn't a container. use single copy operation than w/o recursion.

                        this.GetUnderlyingServiceOrThrow<ICopyChildItem>(out copyChildItem);

                        copyChildItem.CopyChildItem(nodeToCopy, destination);
                    }
                }
            }
            else
            {
                // the copy isn't recursive. Just call the simple copy operation at the source root.
                this.GetUnderlyingServiceOrThrow<ICopyChildItem>(out var copyChildItem);

                copyChildItem.CopyChildItem(nodeToCopy, destination);
            }
        }

        public object? CopyChildItemParameters(string childName, string destination, bool recurse)
            => this.InvokeUnderlyingOrDefault<ICopyChildItem>(copyChildItem => copyChildItem.CopyChildItemParameters(childName, destination, recurse));

        #endregion ICopyChildItemRecursive

        #region IMoveChildItem

        // TODO: The provider gives the original parent to make it easier to remove the connection to the moved node. Necessary?
        public void MoveChildItem(ContainerNode parentOfNodeToMove, ProviderNode nodeToMove, string[] destination)
            => this.InvokeUnderlyingOrThrow<IMoveChildItem>(moveChildItem => moveChildItem.MoveChildItem(parentOfNodeToMove, nodeToMove, destination));

        public object? MoveChildItemParameter(string name, string destination)
            => this.InvokeUnderlyingOrDefault<IMoveChildItem>(moveChildItem => moveChildItem.MoveChildItemParameters(name, destination));

        #endregion IMoveChildItem
    }
}