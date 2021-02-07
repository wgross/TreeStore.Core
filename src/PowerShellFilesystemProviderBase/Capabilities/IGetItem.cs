using System.Management.Automation;

namespace PowerShellFilesystemProviderBase.Capabilities
{
    public interface IGetItem
    {
        /// <summary>
        /// Returns custom parameters to be applied on retrieving the item
        /// </summary>
        /// <param name="childName"></param>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        object? GetItemParameters() => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Creates a <see cref="PSObject"/> warpping the implementing class of this interface in the <see cref="PowerShell"/> pipe.
        /// </summary>
        /// <returns></returns>
        PSObject? GetItem();
    }
}