using System.Diagnostics.CodeAnalysis;

namespace Essentials.NET;


public static class ReadOnlyListExtension
{

    /// <summary>Returns the index of the first occurrence of a given element or -1 if the element is not included.</summary>
    public static int IndexOf<T>(this IReadOnlyList<T> list, T element)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (Equals(element, list[i]))
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>Returns the element at the specified index or the default value if the index is out of bounds.</summary>
    [return: MaybeNull]
    public static T GetOrDefault<T>(this IReadOnlyList<T> list, int index, IReadOnlyList<T>? _ = null /*avoid ambiguity with IList extension method*/)
    {
        if (index > 0 && index < list.Count)
        {
            return list[index];
        }
        return default;
    }

    /// <summary>Retrieves the element at the specified index or the default value if the index is out of bounds. Returns whether the index is out of bounds.</summary>
    public static bool TryGet<T>(this IReadOnlyList<T> list, int index, [MaybeNullWhen(false)] out T value, IReadOnlyList<T>? _ = null /*avoid ambiguity with IList extension method*/)
    {
        if (index > 0 && index < list.Count)
        {
            value = list[index];
            return true;
        }
        value = default;
        return false;
    }

    /// <summary>Returns the predecessor of the given item or the deafault value if there is no predecessor.</summary>
    [return: MaybeNull]
    public static T GetPredecessor<T>(this IReadOnlyList<T> list, T item)
    {
        var index = list.IndexOf(item);
        if (index == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(item));
        }
        if (index - 1 >= 0)
        {
            return list[index - 1];
        }
        return default;
    }

    /// <summary>Returns the successor of the given item or the deafault value if there is no successor.</summary>
    [return: MaybeNull]
    public static T GetSuccessor<T>(this IReadOnlyList<T> list, T item)
    {
        var index = list.IndexOf(item);
        if (index == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(item));
        }
        if (index + 1 < list.Count)
        {
            return list[index + 1];
        }
        return default;
    }

}
