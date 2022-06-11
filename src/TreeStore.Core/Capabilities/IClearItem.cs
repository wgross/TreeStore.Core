﻿using System.Management.Automation;

namespace TreeStore.Core.Capabilities
{
    /// <summary>
    /// Clear the content of a file system item
    /// </summary>
    public interface IClearItem
    {
        /// <summary>
        /// Dynamic parameter provided to PowerShells 'Clear-Item' command.
        /// </summary>
        public object? ClearItemParameters() => new RuntimeDefinedParameterDictionary();

        /// <summary>
        /// Clear the content of the file system item.
        /// </summary>
        public void ClearItem();
    }
}