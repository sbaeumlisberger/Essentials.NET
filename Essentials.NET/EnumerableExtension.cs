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
        if (enumerable.FirstOrDefault() is T first)
        {
            value = first;
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
        return enumerable.Distinct(comparer).Count() <= 1;
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

    public static void ContainsAnyOf<T>(this IEnumerable<T> enumerable, IEnumerable<T> other)
    {
        other.Any(element => enumerable.Contains(element));
    }

}