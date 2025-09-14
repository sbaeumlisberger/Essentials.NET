using System.Diagnostics.CodeAnalysis;

namespace Essentials.NET;

public static class IEnumerableExtensions
{

    /// <summary>Determines whether the sequence contains no elements or not.</summary>
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.Any();
    }

    /// <summary>Retrieves the first element of a sequence, or the default value if the sequence contains no elements. Returns whether the the sequence contains no elements or not.</summary>
    public static bool TryGetFirst<T>(this IEnumerable<T> enumerable, [MaybeNullWhen(false)] out T value)
    {
        foreach (T element in enumerable)
        {
            value = element;
            return true;
        }
        value = default!;
        return false;
    }

    /// <summary>Retrieves the first element of the sequence that satisfies a condition or the default value if no such element is found. Returns whether the such element is found or not.</summary>
    public static bool TryGetFirst<T>(this IEnumerable<T> enumerable, Func<T, bool> predicate, [MaybeNullWhen(false)] out T value)
    {
        foreach (T element in enumerable)
        {
            if (predicate(element))
            {
                value = element;
                return true;
            }
        }
        value = default!;
        return false;
    }

    /// <summary>Flatten the elements of multiple sequences.</summary>
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> enumerable)
    {
        IEnumerable<T> result = Enumerable.Empty<T>();
        foreach (IEnumerable<T> element in enumerable)
        {
            result = result.Concat(element);
        }
        return result;
    }

    /// <summary>Determines whether all elements in the sequence are equal.</summary>
    public static bool AllEqual<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.AllEqual(EqualityComparer<T>.Default);
    }

    /// <summary>Determines whether all elements in the sequence are equal by using a specified <see cref="IEqualityComparer{T}"/> to compare values.</summary>
    public static bool AllEqual<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
    {
        var first = enumerable.FirstOrDefault();
        return enumerable.All(x => comparer.Equals(x, first));
    }

    /// <summary>Creates a dictionary from a sequence of key value pairs.</summary>
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable) where TKey : notnull
    {
        return enumerable.ToDictionary(kv => kv.Key, kv => kv.Value);
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (T element in enumerable)
        {
            action(element);
        }
    }

    public static bool ContainsAnyOf<T>(this IEnumerable<T> enumerable, IEnumerable<T> other)
    {
        return other.Any(element => enumerable.Contains(element));
    }

#if NETSTANDARD2_0
    public static IEnumerable<TSource[]> Chunk<TSource>(this IEnumerable<TSource> source, int size)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        if (size < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(size));
        }

        using IEnumerator<TSource> e = source.GetEnumerator();
        while (e.MoveNext())
        {
            TSource[] chunk = new TSource[size];
            chunk[0] = e.Current;

            int i = 1;
            for (; i < chunk.Length && e.MoveNext(); i++)
            {
                chunk[i] = e.Current;
            }

            if (i == chunk.Length)
            {
                yield return chunk;
            }
            else
            {
                Array.Resize(ref chunk, i);
                yield return chunk;
                yield break;
            }
        }
    }
#endif

    /// <summary>Returns the predecessor of the given element or the deafault value if there is no predecessor.</summary>
    [return: MaybeNull]
    public static T GetPredecessor<T>(this IEnumerable<T> enumerable, T element)
    {
        if (enumerable is IList<T> list)
        {
            var index = list.IndexOf(element);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(element));
            }
            if (index - 1 >= 0)
            {
                return list[index - 1];
            }
            return default;
        }
        else if (enumerable is IReadOnlyList<T> readOnlyList)
        {
            var index = readOnlyList.IndexOf(element);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(element));
            }
            if (index - 1 >= 0)
            {
                return readOnlyList[index - 1];
            }
            return default;
        }
        else
        {
            T? predecessor = default;

            foreach (T current in enumerable)
            {
                if (Equals(current, element))
                {
                    return predecessor;
                }
                predecessor = current;
            }

            throw new ArgumentOutOfRangeException(nameof(element));
        }
    }

    /// <summary>Returns the successor of the given element or the default value if there is no successor.</summary>
    [return: MaybeNull]
    public static T GetSuccessor<T>(this IEnumerable<T> enumerable, T element)
    {
        if (enumerable is IList<T> list)
        {
            var index = list.IndexOf(element);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(element));
            }
            if (index + 1 < list.Count)
            {
                return list[index + 1];
            }
            return default;
        }
        else if (enumerable is IReadOnlyList<T> readOnlyList)
        {
            var index = readOnlyList.IndexOf(element);
            if (index == -1)
            {
                throw new ArgumentOutOfRangeException(nameof(element));
            }
            if (index + 1 < readOnlyList.Count)
            {
                return readOnlyList[index + 1];
            }
            return default;
        }
        else
        {
            bool elementPassed = false;

            foreach (T current in enumerable)
            {
                if (elementPassed)
                {
                    return current;
                }
                if (Equals(current, element))
                {
                    elementPassed = true;
                }
            }

            if (!elementPassed)
            {
                throw new ArgumentOutOfRangeException(nameof(element));
            }

            return default;
        }
    }
}