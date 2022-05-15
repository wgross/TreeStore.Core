using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    public interface INewItemProperty
    {
        /// <summary>
        /// Returns custom parameters for property creation. These will extend the set of parameters accepted by the New-ItemProperty
        /// command. By default an empty <see cref="RuntimeDefinedParameterDictionary"/> is returned.
        /// </summary>
        object? NewItemPropertyParameters(string propertyName, string? propertyTypeName, object? value) => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Creates a new item property named <paramref name="propertyName"/> having the type <paramref name="propertyTypeName"/>.
        /// The property receives the initial value of <paramref name="value"/>.
        /// </summary>
        void NewItemProperty(string propertyName, string? propertyTypeName, object? value);
    }
}