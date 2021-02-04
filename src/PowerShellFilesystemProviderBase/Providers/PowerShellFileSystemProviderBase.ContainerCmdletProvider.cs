using PowerShellFilesystemProviderBase.Capabilities;
using System.IO;
using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Providers
{
    public partial class PowerShellFileSystemProviderBase
    {
        protected override bool ConvertPath(string path, string filter, ref string updatedPath, ref string updatedFilter)
        {
            return base.ConvertPath(path, filter, ref updatedPath, ref updatedFilter);
        }

        protected override void CopyItem(string path, string copyPath, bool recurse)
        {
            base.CopyItem(path, copyPath, recurse);
        }

        protected override object CopyItemDynamicParameters(string path, string destination, bool recurse)
        {
            return base.CopyItemDynamicParameters(path, destination, recurse);
        }

        protected override void GetChildItems(string path, bool recurse)
        {
            base.GetChildItems(path, recurse);
        }

        protected override void GetChildItems(string path, bool recurse, uint depth)
        {
            if (this.TryGetNodeByPath<IGetChildItems>(path, out var getChildItems))
                this.GetChildItems(parentGetChildItems: getChildItems, path, recurse, depth);
        }

        private void GetChildItems(IGetChildItems parentGetChildItems, string path, bool recurse, uint depth)
        {
            foreach (var childGetItem in parentGetChildItems.GetChildItems())
            {
                var childItemPSObject = childGetItem.GetItem();
                if (childItemPSObject is not null)
                {
                    var childItemPath = Path.Join(path, childGetItem.Name);
                    this.WriteItemObject(
                        item: this.DecorateItem(childItemPath, childItemPSObject),
                        path: this.DecoratePath(childItemPath),
                        isContainer: childGetItem.IsContainer);

                    //TODO: recurse in cmdlet provider will be slow if the underlying model could optimize fetching of data.
                    // alternatives:
                    // - let first container pull in the whole operation -> change IGetChildItems to GetChildItems( bool recurse, uint depth)
                    // - notify first container of incoming request so it can prepare the fetch: IPrepareGetChildItems: Prepare(bool recurse, uint depth) then resurce in provider
                    //   General solution would be to introduce a call context to allow an impl. to inspect the original request.
                    if (recurse && childGetItem is IGetChildItems childGetChildItems)
                        this.GetChildItems(childGetChildItems, childItemPath, recurse, depth);
                }
            }
        }

        protected override object? GetChildItemsDynamicParameters(string path, bool recurse)
        {
            if (this.TryGetNodeByPath<IGetChildItems>(path, out var getChildItems))
            {
                return getChildItems.GetChildItemParameters();
            }
            return null;
        }

        protected override void GetChildNames(string path, ReturnContainers returnContainers)
        {
            base.GetChildNames(path, returnContainers);
        }

        protected override object GetChildNamesDynamicParameters(string path)
        {
            return base.GetChildNamesDynamicParameters(path);
        }

        protected override bool HasChildItems(string path)
        {
            return base.HasChildItems(path);
        }

        protected override void RemoveItem(string path, bool recurse)
        {
            base.RemoveItem(path, recurse);
        }

        protected override object RemoveItemDynamicParameters(string path, bool recurse)
        {
            return base.RemoveItemDynamicParameters(path, recurse);
        }

        protected override void NewItem(string path, string itemTypeName, object newItemValue)
        {
            base.NewItem(path, itemTypeName, newItemValue);
        }

        protected override object NewItemDynamicParameters(string path, string itemTypeName, object newItemValue)
        {
            return base.NewItemDynamicParameters(path, itemTypeName, newItemValue);
        }

        protected override void RenameItem(string path, string newName)
        {
            base.RenameItem(path, newName);
        }

        protected override object RenameItemDynamicParameters(string path, string newName)
        {
            return base.RenameItemDynamicParameters(path, newName);
        }
    }
}