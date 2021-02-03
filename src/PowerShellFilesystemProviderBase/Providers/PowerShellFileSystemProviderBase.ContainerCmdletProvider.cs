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
            {
                foreach (var getItem in getChildItems.GetChildItems())
                {
                    var pso = getItem.GetItem();
                    if (pso is not null)
                    {
                        var itemPath = Path.Join(path, getItem.Name);
                        this.WriteItemObject(
                            item: this.DecorateItem(itemPath, pso),
                            path: this.DecoratePath(itemPath),
                            isContainer: getItem.IsContainer);
                    }
                }
            };
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