using TreeStore.Core.Capabilities;
using TreeStore.Core.Nodes;
using System;
using System.IO;
using System.Management.Automation;

namespace TreeStore.Core.Providers
{
    public partial class TreeStoreCmdletProviderBase
    {
        protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
        {
            return base.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
        }

        /// <summary>
        /// Implements the copying of a provider item.
        /// The existence of the child node <paramref name="path"/> has already happened in <see cref="ItemExists(string)"/>.
        /// th destination path <paramref name="destination"/> is completely unverified.
        /// </summary>
        protected override void CopyItem(string path, string destination, bool recurse)
        {
            var (parentPath, childName) = new PathTool().SplitParentPathAndChildName(path);

            this.InvokeContainerNodeOrDefault(
                path: parentPath,
                invoke: sourceParentNode =>
                {
                    // first check that node to copy exists
                    if (!sourceParentNode.TryGetChildNode(childName!, out var nodeToCopy))
                        throw new InvalidOperationException($"Item '{path}' doesn't exist");

                    // find the deepest ancestor which serves as a destination to copy to
                    var destinationAncestor = this.GetDeepestNodeByPath(destination, out var missingPath);

                    if (destinationAncestor is ContainerNode destinationAncestorContainer)
                    {
                        // destination ancestor is a container and might accept the copy
                        destinationAncestorContainer.CopyChildItem(nodeToCopy, destination: missingPath, recurse);
                    }
                    else
                    {
                        base.CopyItem(path, destination, recurse);
                    }
                },
                fallback: () => base.CopyItem(path, destination, recurse));
        }

        protected override object? CopyItemDynamicParameters(string path, string destination, bool recurse) => this.InvokeContainerNodeOrDefault(
            path: new PathTool().SplitProviderPath(path).path.items,
            invoke: c => c.CopyChildItemParameters(path, destination, recurse),
            fallback: () => base.CopyItemDynamicParameters(path, destination, recurse));

        protected override void GetChildItems(string path, bool recurse)
        {
            base.GetChildItems(path, recurse);
        }

        protected override void GetChildItems(string path, bool recurse, uint depth)
        {
            void writeItemObject(PSObject item, string path, string name, bool isContainer) => this.WriteItemObject(item, path, isContainer);

            this.InvokeContainerNodeOrDefault(
              path: new PathTool().SplitProviderPath(path).path.items,
              invoke: c => this.GetChildItems(parentContainer: c, path, recurse, depth, writeItemObject),
              fallback: () => base.GetChildItems(path, recurse, depth));
        }

        private void GetChildItems(ContainerNode parentContainer, string path, bool recurse, uint depth, Action<PSObject, string, string, bool> writeItemObject)
        {
            foreach (var childGetItem in parentContainer.GetChildItems())
            {
                var childItemPSObject = childGetItem.GetItem();
                if (childItemPSObject is not null)
                {
                    var childItemPath = Path.Join(path, childGetItem.Name);
                    writeItemObject(childItemPSObject, childItemPath, childGetItem.Name, childGetItem is ContainerNode);

                    //TODO: recurse in cmdlet provider will be slow if the underlying model could optimize fetching of data.
                    // alternatives:
                    // - let first container pull in the whole operation -> change IGetChildItems to GetChildItems( bool recurse, uint depth)
                    // - notify first container of incoming request so it can prepare the fetch: IPrepareGetChildItems: Prepare(bool recurse, uint depth) then resurce in provider
                    //   General solution would be to introduce a call context to allow an impl. to inspect the original request.
                    if (recurse && depth > 0 && childGetItem is ContainerNode childContainer)
                        this.GetChildItems(childContainer, childItemPath, recurse, depth - 1, writeItemObject);
                }
            }
        }

        protected override object? GetChildItemsDynamicParameters(string path, bool recurse) => this.InvokeContainerNodeOrDefault(
            path: new PathTool().SplitProviderPath(path).path.items,
            invoke: c => c.GetChildItemParameters(path, recurse),
            fallback: () => base.GetChildItemsDynamicParameters(path, recurse));

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            void writeItemObject(PSObject _, string path, string name, bool isContainer) => this.WriteItemObject(name, path, isContainer);

            this.InvokeContainerNodeOrDefault(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: c => this.GetChildItems(parentContainer: c, path, recurse: false, depth: 0, writeItemObject),
                fallback: () => base.GetChildNames(path, returnContainers));
        }

        protected override object GetChildNamesDynamicParameters(string path)
        {
            return base.GetChildNamesDynamicParameters(path);
        }

        protected override bool HasChildItems(string path)
        {
            return this.InvokeProviderNodeOrDefault<bool>(
                path: new PathTool().SplitProviderPath(path).path.items,
                invoke: n => n switch
                {
                    ContainerNode c => c.HasChildItems(),

                    // LeafNodes never have children
                    _ => false
                },
                fallback: () => base.HasChildItems(path));
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            var (parentPath, childName) = new PathTool().SplitParentPathAndChildName(path);

            this.InvokeContainerNodeOrDefault(
                path: parentPath,
                invoke: c => c.RemoveChildItem(childName!, recurse),
                fallback: () => base.RemoveItem(path, recurse));
        }

        protected override object? RemoveItemDynamicParameters(string path, bool recurse)
        {
            var (parentPath, childName) = new PathTool().SplitParentPathAndChildName(path);

            return this.InvokeContainerNodeOrDefault(parentPath,
               invoke: c => c.RemoveChildItemParameters(childName!, recurse),
               fallback: () => base.RemoveItemDynamicParameters(path, recurse));
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            var (parentPath, childName) = new PathTool().SplitParentPathAndChildName(path);
            if (TryGetNodeByPath<INewChildItem>(this.DriveInfo.RootNode, parentPath, out _, out var newChildItem))
            {
                var resultNode = newChildItem.NewChildItem(childName!, itemTypeName, newItemValue);
                if (resultNode is not null)
                {
                    this.WriteProviderNode(path, resultNode);
                }
            }
        }

        protected override object? NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
        {
            var (parentPath, childName) = new PathTool().SplitParentPathAndChildName(path);

            return this.InvokeContainerNodeOrDefault(parentPath,
                invoke: c => c.NewChildItemParameters(childName!, itemTypeName, newItemValue),
                fallback: () => base.NewItemDynamicParameters(path, itemTypeName, newItemValue));
        }

        protected override void RenameItem(string path, string newName)
        {
            var (parentPath, childName) = new PathTool().SplitParentPathAndChildName(path);
            if (TryGetNodeByPath<IRenameChildItem>(this.DriveInfo.RootNode, parentPath, out _, out var renameChildItem))
            {
                renameChildItem.RenameChildItem(childName!, newName);
            }
        }

        protected override object? RenameItemDynamicParameters(string path, string newName)
        {
            var (parentPath, childName) = new PathTool().SplitParentPathAndChildName(path);

            return this.InvokeContainerNodeOrDefault(parentPath,
                invoke: c => c.RenameChildItemParameters(childName!, newName),
                fallback: () => base.RenameItemDynamicParameters(path, newName));
        }
    }
}