namespace System.Collections.Generic;

public static class DictionaryExtensions
{
    public static bool TryGetValue<T>(this IDictionary<string, object?> dictionary, string key, [NotNullWhen(true)] out T? value)
    {
        if (dictionary.TryGetValue(key, out var objectValue))
        {
            if (objectValue is T t)
            {
                value = t;
                return true;
            }
        }
        value = default;
        return false;
    }

    public static IDictionary<string, object?> AsDictionary(this IDictionary<string, object?> dict, string key)
    {
        return (IDictionary<string, object?>)dict[key]!;
    }

    public static IDictionary<string, object?> CloneShallow(this IDictionary<string, object?> thisDictionary)
    {
        return thisDictionary
            .Where(kv => !IsContainerChild(kv.Value))
            .Aggregate(new Dictionary<string, object?>(), (dict, kv) =>
             {
                 dict.Add(kv.Key, kv.Value);
                 return dict;
             });
    }

    private static bool IsContainerChild(object? value)
    {
        return value switch
        {
            IDictionary<string, object?> dict => true,
            _ => false
        };
    }
}