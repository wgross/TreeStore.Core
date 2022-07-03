namespace TreeStore.Core.Providers;

public partial class TreeStoreCmdletProviderBase
{
    protected override void ClearItem(string path)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            node.ClearItem(provider: this);
        }
    }

    protected override object? ClearItemDynamicParameters(string path)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            return node.ClearItemParameters();
        }
        return null;
    }

    protected override void SetItem(string path, object value)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            node.SetItem(provider: this, value);
        }
    }

    protected override object? SetItemDynamicParameters(string path, object value)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            return node.SetItemParameters();
        }
        return null;
    }

    protected override string[] ExpandPath(string path)
    {
        return base.ExpandPath(path);
    }

    protected override void InvokeDefaultAction(string path)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            node.InvokeItem(provider: this);
        }
    }

    protected override object? InvokeDefaultActionDynamicParameters(string path)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            return node.InvokeItemParameters(provider: this);
        }
        else return null;
    }

    protected override void GetItem(string path)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            this.WriteProviderNode(path, node);
        }
    }

    protected override object? GetItemDynamicParameters(string path)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            return node.GetItemParameters();
        }
        else return null;
    }

    protected override bool IsValidPath(string path)
    {
        throw new System.NotImplementedException();
    }

    protected override bool ItemExists(string path)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            return node.ItemExists(provider: this);
        }
        return false;
    }

    protected override object? ItemExistsDynamicParameters(string path)
    {
        if (this.TryGetNodeByPath(path, out var node))
        {
            return node.ItemExistsParameters(provider: this);
        }
        else return null;
    }
}