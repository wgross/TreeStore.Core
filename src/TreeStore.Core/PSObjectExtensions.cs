namespace TreeStore.Core;

public static class PSObjectExtensions
{
    /// <summary>
    /// Reads the <see cref="PSObject.BaseObject"/> and casts it to <typeparamref name="T"/>.
    /// </summary>
    public static T Unwrap<T>(this PSObject pso) => (T)pso.BaseObject;

    /// <summary>
    /// Reads the value <paramref name="name"/> from the given <see cref="PSObject"/> and casts it to <typeparamref name="V"/>.
    /// </summary>

    public static V Property<V>(this PSObject obj, string name) => (V)obj.Properties[name].Value;

    /// <summary>
    /// Reads the value <paramref name="name"/> from the given <see cref="PSObject"/> and casts it to <typeparamref name="V"/>.
    /// </summary>
    public static bool PropertyIsNull(this PSObject obj, string name) => obj.Properties[name] is null;
}