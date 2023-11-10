using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Essentials.NET;


public static class DictionaryExtension
{

    /// <summary>Retrieves the value for the given key. If there is no entry <code>default (TValue)</code> is returned.</summary>
    [return: MaybeNull]
    public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        return default;
    }

    public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TKey : notnull where TValue : new()
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        value = new TValue();
        dictionary.Add(key, value);
        return value;
    }

    /// <summary>Retrieves the value for the given key. If there is no entry the given value will be added.</summary>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue addValue) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        dictionary.Add(key, addValue);
        return addValue;
    }

    /// <summary>Retrieves the value for the given key. If there is no entry the return value of the given value factory function will be added.</summary>
    public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, Func<TValue> valueFactory) where TKey : notnull
    {
        if (dictionary.TryGetValue(key, out var value))
        {
            return value;
        }
        value = valueFactory();
        dictionary.Add(key, value);
        return value;
    }

    /// <summary>Removes all entries from the dictionary machting the given predicate.</summary>
    public static void RemoveAll<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate) where TKey : notnull
    {
        var keysToRemove = new List<TKey>();
        foreach (var entry in dictionary)
        {
            if (predicate(entry))
            {
                keysToRemove.Add(entry.Key);
            }
        }
        foreach (var key in keysToRemove)
        {
            dictionary.Remove(key);
        }
    }

    /// <summary>Removes the first entry from the dictionary machting the given predicate.</summary>
    public static void RemoveFirst<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, Predicate<KeyValuePair<TKey, TValue>> predicate) where TKey : notnull
    {
        foreach (var entry in dictionary)
        {
            if (predicate(entry))
            {
                dictionary.Remove(entry);
                return;
            }
        }
        throw new Exception("No entry machting the given predicate.");
    }

    public static void RemoveRange<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, IEnumerable<TKey> keys) where TKey : notnull
    {
        keys.ForEach(key => dictionary.Remove(key));
    }

    /// <summary>
    /// Returns a read-only wrapper for the current dictionary.
    /// </summary>
    public static IReadOnlyDictionary<TKey, TValue> AsReadonly<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        return new ReadOnlyDictionary<TKey, TValue>(dictionary);
    }

}