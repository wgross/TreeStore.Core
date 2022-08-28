namespace TreeStore.Core.Capabilities
{
    public interface IClearItemContent
    {
        /// <summary>
        /// Returns custom parameters to be applied for clearing an items content.
        /// </summary>
        /// <returns>empty <see cref="RuntimeDefinedParameterDictionary"/> by default</returns>
        public object? ClearItemContentParameters() => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Removes the content from the item.
        /// </summary>
        public void ClearItemContent(ICmdletProvider provider);
    }
}