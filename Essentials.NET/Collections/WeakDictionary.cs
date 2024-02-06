using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Essentials.NET;


/// <summary>
/// A generic dictionary, which allows the values to be garbage collected if there are no other references 
/// to them than from the dictionary itself. If the value of a particular entry in the dictionary has 
/// been collected, then the entry becomes effectively unreachable. To allow garbage collection of unused 
/// System.WeakReference objects the Cleanup method must be called. By default a cleanup is executed before
/// every insert of new entires. 
/// </summary>
public class WeakDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TValue : class where TKey : notnull
{

    public TValue this[TKey key]
    {
        get => dictionary[key].TryGetTarget(out var value) ? value : throw new KeyNotFoundException($"\"{key}\" not found");
        set => dictionary[key] = new WeakReference<TValue>(value);
    }

    public int Count => this.Count();

    public ICollection<TKey> Keys => this.Select(item => item.Key).ToList();

    public ICollection<TValue> Values => this.Select(item => item.Value).ToList();

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => this.Select(item => item.Key).ToList();

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => this.Select(item => item.Value).ToList();

    public bool IsReadOnly => false;

    private readonly Dictionary<TKey, WeakReference<TValue>> dictionary = new Dictionary<TKey, WeakReference<TValue>>();

    /// <summary>
    /// Indicates whether a cleanup is executed before accessing new entries.
    /// </summary>
    public bool AutoCleanup { get; set; } = true;

    public WeakDictionary()
    {
    }

    public WeakDictionary(IEnumerable<KeyValuePair<TKey, TValue>> source)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }
        this.AddRange(source);
    }

    /// <summary>
    /// Allows the garbage collection of unused System.WeakReference objects.
    /// </summary>
    public void Cleanup()
    {
        dictionary.RemoveAll(entry => !entry.Value.TryGetTarget(out _));
    }

    public void Add(TKey key, TValue value)
    {
        if (key == null)
        {
            throw new ArgumentNullException(nameof(key));
        }
        if (value == null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (AutoCleanup)
        {
            Cleanup();
        }
        else if (dictionary.TryGetValue(key, out var weakReference) && !weakReference.TryGetTarget(out _))
        {
            weakReference.SetTarget(value);
            return;
        }

        dictionary.Add(key, new WeakReference<TValue>(value));
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        Add(item.Key, item.Value);
    }

    public bool ContainsKey(TKey key)
    {
        return TryGetValue(key, out _);
    }

    public bool Contains(TValue value)
    {
        return this.Any(item => Equals(item.Value, value));
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return TryGetValue(item.Key, out var value) && Equals(value, item.Value);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (dictionary.TryGetValue(key, out var weakReference))
        {
            return weakReference.TryGetTarget(out value);
        }
        value = default;
        return false;
    }

    public bool Remove(TKey key)
    {
        if (dictionary.TryGetValue(key, out var weakReference))
        {
            dictionary.Remove(key);
            return weakReference.TryGetTarget(out _);
        }
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (Contains(item))
        {
            dictionary.Remove(item.Key);
            return true;
        }
        return false;
    }

    public void Clear()
    {
        dictionary.Clear();
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        this.ToArray().CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach (var item in dictionary)
        {
            if (item.Value.TryGetTarget(out var value))
            {
                yield return new KeyValuePair<TKey, TValue>(item.Key, value);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

}