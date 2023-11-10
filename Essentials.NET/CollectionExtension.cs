using System.Collections;

namespace Essentials.NET;


public static class CollectionExtension
{

    /// <summary>Adds the elements of the specified sequence to the end of the collection.</summary>
    public static void AddRange<TValue>(this ICollection<TValue> collection, IEnumerable<TValue> source)
    {
        foreach (TValue item in source)
        {
            collection.Add(item);
        }
    }

    /// <summary>Removes the elements of the specified sequence from the collection.</summary>
    public static void RemoveRange<TValue>(this ICollection<TValue> collection, IEnumerable<TValue> source)
    {
        foreach (TValue item in source)
        {
            collection.Remove(item);
        }
    }

    /// <summary>Replaces the elements of the collection with the the elements of the specified sequence.</summary>
    public static void ReplaceAll<TValue>(this ICollection<TValue> collection, IEnumerable<TValue> source)
    {
        collection.Clear();
        foreach (TValue item in source)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Returns a read-only wrapper for the current collection.
    /// </summary>
    public static IReadOnlyCollection<T> AsReadonly<T>(this ICollection<T> collection)
    {
        return new ReadOnlyCollection<T>(collection);
    }

    private class ReadOnlyCollection<T> : IReadOnlyCollection<T>
    {

        public int Count => collection.Count;

        private readonly ICollection<T> collection;

        public ReadOnlyCollection(ICollection<T> collection)
        {
            this.collection = collection;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return collection.GetEnumerator();
        }
    }

}