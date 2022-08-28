namespace TreeStore.Core.Capabilities
{
    public interface IGetItemContent
    {
        /// <summary>
        /// Returns custom parameters to be applied for the getting an items content
        /// </summary>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        public object? GetItemContentParameters() => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Retrieves the content from an item.
        /// </summary>
        public IContentReader? GetItemContentReader(ICmdletProvider provider);
    }
}