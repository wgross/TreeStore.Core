using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface IRemoveChildItem
    {
        /// <summary>
        /// Returns custom parameters to be applied for the removal of the child node <paramref name="childName"/>
        /// </summary>
        /// <param name="childName"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        public object? RemoveChildItemParameters(string childName, bool recurse) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// removes the child item specify by <paramref name="childName"/>
        /// </summary>
        /// <param name="childName"></param>
        public void RemoveChildItem(string childName, bool recurse);
    }
}