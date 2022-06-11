using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface ISetItemProperty
    {
        /// <summary>
        /// Returns custom parameters to be applied for the setting ofitem properties in
        /// </summary>
        /// <param name="properties"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        public object? SetItemPropertyParameters(PSObject properties) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Set the given item properties
        /// </summary>
        /// <param name="properties"></param>
        public void SetItemProperty(PSObject properties);
    }
}