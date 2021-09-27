using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IRemoveChildItem : IItemExists
    {
        /// <summary>
        /// Returns custom parameters to be applied for the removal of the child node <paramref name="childName"/>
        /// </summary>
        /// <param name="childName"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? RemoveChildItemParameters(string childName, bool recurse) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// removes the cild item specify by <paramref name="childName"/>
        /// </summary>
        /// <param name="childName"></param>
        void RemoveChildItem(string childName, bool recurse);
    }
}