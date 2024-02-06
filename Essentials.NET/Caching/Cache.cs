namespace Essentials.NET;

/// <summary>
/// A simple cache with a maximum size. If the cache size is exceeded, the entry that has not been used for the longest time is removed.
/// </summary>
public class Cache<TKey, TValue> where TKey : notnull
{
    public int MaxSize { get; }

    public int CurrentSize => cacheDictionary.Count;

    public IEnumerable<TValue> Values => cacheDictionary.Values;

    private readonly OrderedDictionary<TKey, TValue> cacheDictionary;

    private readonly Action<TValue>? removedCallback;

    public Cache(int maxSize, Action<TValue>? removedCallback = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSize, 1);
        MaxSize = maxSize;
        this.removedCallback = removedCallback;
        cacheDictionary = new OrderedDictionary<TKey, TValue>(maxSize);
    }

    public bool Remove(TKey key)
    {
        lock (cacheDictionary)
        {
            if (cacheDictionary.TryGetValue(key, out var value))
            {
                cacheDictionary.Remove(key);
                removedCallback?.Invoke(value);
                return true;
            }
            return false;
        }
    }

    public TValue GetOrCreate(TKey key, Func<TKey, TValue> createValueCallback)
    {
        lock (cacheDictionary)
        {
            if (cacheDictionary.TryGetValue(key, out var value))
            {
                if (!Equals(value, cacheDictionary[cacheDictionary.Count - 1]))
                {
                    cacheDictionary.Remove(key);
                    cacheDictionary.Add(key, value);
                }
            }
            else
            {
                if (cacheDictionary.Count == MaxSize)
                {
                    RemoveOldestCacheEntry();
                }
                value = createValueCallback.Invoke(key);
                cacheDictionary.Add(key, value);
            }
            return value;
        }
    }

    private void RemoveOldestCacheEntry()
    {
        var value = cacheDictionary[0];
        cacheDictionary.RemoveAt(0);
        removedCallback?.Invoke(value);
    }

}
