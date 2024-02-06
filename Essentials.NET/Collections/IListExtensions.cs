using System.Collections;
using System.Collections.ObjectModel;

namespace Essentials.NET;

public static class IListExtensions
{
    /// <summary>Removes the first element in the list.</summary>
    public static bool RemoveFirst(this IList list)
    {
        if (list.Count > 0)
        {
            list.RemoveAt(0);
            return true;
        }
        return false;
    }

    /// <summary>Removes the first element in the list that satisfies the given predicate.</summary>
    public static bool RemoveFirst<T>(this IList<T> list, Predicate<T> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
            {
                list.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    /// <summary>Removes the last element in the list.</summary>
    public static bool RemoveLast<T>(this IList<T> list)
    {
        if (list.Count > 0)
        {
            list.RemoveAt(list.Count - 1);
            return true;
        }
        return false;
    }

    /// <summary>Removes all elements from the list that satisfy the given predicate.</summary>
    public static void RemoveAll<T>(this IList<T> list, Predicate<T> predicate)
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            if (predicate(list[i]))
            {
                list.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Replaces a element of the list with a new element.
    /// </summary>
    public static void Replace<T>(this IList<T> list, T oldElement, T newElement)
    {
        int index = list.IndexOf(oldElement);
        if (index == -1)
        {
            throw new ArgumentOutOfRangeException(nameof(oldElement));
        }
        list[index] = newElement;
    }

    /// <summary>
    /// Returns a read-only wrapper for the current list.
    /// </summary>
    public static IReadOnlyList<T> AsReadonly<T>(this IList<T> list)
    {
        return new ReadOnlyCollection<T>(list);
    }

}