using System.Collections;
using System.Diagnostics.CodeAnalysis;


namespace Essentials.NET;

public class OrderedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : notnull
{
    public TValue this[TKey key]
    {
        get => TryGetValue(key, out var value) ? value : throw new KeyNotFoundException($"\"{key}\" not found");
        set => internalDic[key] = value;
    }

    public TValue this[int index]
    {
        get => (TValue)internalDic[index]!;
        set => internalDic[index] = value;
    }

    public ICollection<TKey> Keys => internalDic.Keys.Cast<TKey>().ToList();

    public ICollection<TValue> Values => internalDic.Values.Cast<TValue>().ToList();

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => internalDic.Keys.Cast<TKey>();

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => internalDic.Values.Cast<TValue>();

    public int Count => internalDic.Count;

    public bool IsReadOnly => internalDic.IsReadOnly;

    private readonly System.Collections.Specialized.OrderedDictionary internalDic;

    public OrderedDictionary()
    {
        internalDic = new System.Collections.Specialized.OrderedDictionary();
    }

    public OrderedDictionary(int capacity)
    {
        internalDic = new System.Collections.Specialized.OrderedDictionary(capacity);
    }

    public void Add(TKey key, TValue value)
    {
        internalDic.Add(key, value);
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
        internalDic.Add(item.Key, item.Value);
    }

    public void Insert(int index, object key, object? value)
    {
        internalDic.Insert(index, key, value);
    }

    public void Clear()
    {
        internalDic.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        return internalDic.Contains(item.Key);
    }

    public bool ContainsKey(TKey key)
    {
        return internalDic.Contains(key);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        internalDic.CopyTo(array, arrayIndex);
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        var enumerator = internalDic.GetEnumerator();
        while (enumerator.MoveNext())
        {
            yield return new KeyValuePair<TKey, TValue>((TKey)enumerator.Key, (TValue)enumerator.Value!);
        }
    }

    public bool Remove(TKey key)
    {
        if (internalDic.Contains(key))
        {
            internalDic.Remove(key);
            return true;
        }
        return false;
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        if (internalDic.Contains(item.Key))
        {
            internalDic.Remove(item.Key);
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        internalDic.RemoveAt(index);
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        if (internalDic.Contains(key))
        {
            value = (TValue)internalDic[key]!;
            return true;
        }
        value = default;
        return false;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return internalDic.GetEnumerator();
    }

}
