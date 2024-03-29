﻿namespace TreeStore.Core;

public static class ServiceProviderExtensions
{
    /// <summary>
    /// A node that doesn't provider  the capability <see cref="IGetChildItem"/> can't be a container node.
    /// </summary>
    public static bool IsContainer(this IServiceProvider? nodeServices) => nodeServices?.GetService(typeof(IGetChildItem)) is not null;
}