using System.Diagnostics.CodeAnalysis;

namespace Essentials.NET;


public static class EnumerableExtension
{

    /// <summary>Determines whether the sequence contains no elements or not.</summary>
    public static bool IsEmpty<T>(this IEnumerable<T> enumerable)
    {
        return !enumerable.Any();
    }

    /// <summary>Retrieves the first element of a sequence, or the default value if the sequence contains no elements. Returns whether the the sequence contains no elements or not.</summary>
    public static bool TryGetFirst<T>(this IEnumerable<T> enumerable, [MaybeNullWhen(false)] out T value)
    {
        if (enumerable.Any())
        {
            value = enumerable.First();
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

    /// <summary>
    /// Determines whether two sequences contains the same elements by using the default equality comparer and without considering the order.
    /// </summary>
    public static bool ElementsEqual<T>(this IEnumerable<T> enumerable, IEnumerable<T> other)
    {
        return enumerable.ElementsEqual(other, EqualityComparer<T>.Default);
    }

    /// <summary>
    /// Determines whether two sequences contains the same elements by using a specified <see cref="IEqualityComparer{T}"/> to compare values and without considering the order.
    /// </summary>
    public static bool ElementsEqual<T>(this IEnumerable<T> enumerable, IEnumerable<T> other, IEqualityComparer<T> comparer)
    {
        return enumerable.Count() == other.Count() && enumerable.Except(other, comparer).IsEmpty();
    }

    /// <summary>Determines whether all elements in the sequence are equal.</summary>
    public static bool AllEqual<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.AllEqual(EqualityComparer<T>.Default);
    }

    /// <summary>Determines whether all elements in the sequence are equal by using a specified <see cref="IEqualityComparer{T}"/> to compare values.</summary>
    public static bool AllEqual<T>(this IEnumerable<T> enumerable, IEqualityComparer<T> comparer)
    {
        return enumerable.Distinct(comparer).Count() <= 1;
    }

    /// <summary>Retrieves the values of an sequence of key value pairs.</summary>
    public static IEnumerable<TValue> GetValues<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
        return enumerable.Select(kv => kv.Value);
    }

    /// <summary>Retrieves the keys of an sequence of key value pairs.</summary>
    public static IEnumerable<TKey> GetKeys<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
    {
        return enumerable.Select(kv => kv.Key);
    }

    /// <summary>Creates a dictionary from a sequence of key value pairs.</summary>
    public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> enumerable) where TKey : notnull
    {
        Dictionary<TKey, TValue> copy = new Dictionary<TKey, TValue>();
        foreach (KeyValuePair<TKey, TValue> kv in enumerable)
        {
            copy.Add(kv.Key, kv.Value);
        }
        return copy;
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach (T element in enumerable)
        {
            action(element);
        }
    }

    public static void ContainsAny<T>(this IEnumerable<T> enumerable, IEnumerable<T> other)
    {
        other.Any(element => enumerable.Contains(element));

    }

}