using System.Management.Automation;
using System.Management.Automation.Provider;

namespace TreeStore.Core.Capabilities;

public interface IRenameItemProperty
{
    /// <summary>
    /// Returns custom parameters to be applied for the renaming of item properties
    /// </summary>
    /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
    public object? RenameItemPropertyParameters(string sourceProperty, string destinationProperty) => new RuntimeDefinedParameterDictionary();

    /// <summary>
    /// Rename the given item properties
    /// </summary>
    public void RenameItemProperty(CmdletProvider provider, string sourceProperty, string destinationProperty);
}