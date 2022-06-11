using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface IRemoveItemProperty
    {
        /// <summary>
        /// Returns custom parameters to be applied for the removal of item properties
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        public object? RemoveItemPropertyParameters(string propertyName) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Removes the given item properties
        /// </summary>
        /// <param name="properties"></param>
        public void RemoveItemProperty(string propertyName);
    }
}