namespace TreeStore.Core.Capabilities
{
    public interface ISetItemContent
    {
        /// <summary>
        /// Returns custom parameters to be applied for the getting an items content
        /// </summary>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        public object? SetItemContentParameters() => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Retrieves the content from an item.
        /// </summary>
        public IContentWriter? GetItemContentWriter(ICmdletProvider provider);
    }
}