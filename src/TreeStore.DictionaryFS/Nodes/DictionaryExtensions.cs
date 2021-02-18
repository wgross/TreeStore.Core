using System.Collections.Generic;

namespace TreeStore.DictionaryFS.Nodes
{
    public static class DictionaryExtensions
    {
        public static IDictionary<string, object?> AsDictionary(this IDictionary<string, object?> dict, string key)
        {
            return (IDictionary<string, object?>)dict[key]!;
        }
    }
}