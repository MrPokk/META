using System;
using System.Collections.Generic;

public static class ExtensionDictionary
{
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> valueFactory)
        where TKey : notnull
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = valueFactory(key);
            dictionary.Add(key, value);
        }
        return value;
    }
}
