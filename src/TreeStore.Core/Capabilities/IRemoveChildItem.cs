using System.Management.Automation;
using System.Management.Automation.Provider;

namespace TreeStore.Core.Capabilities;

public interface IRemoveChildItem
{
    /// <summary>
    /// Returns custom parameters to be applied for the removal of the child node <paramref name="childName"/>
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? RemoveChildItemParameters(string childName, bool recurse) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// removes the child item specify by <paramref name="childName"/>
    /// </summary>
    public void RemoveChildItem(CmdletProvider provider, string childName, bool recurse);
}