using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Essentials.NET;


public static class ListExtension
{

    /// <summary>Adds an element at the beginning of the list.</summary>
    public static void Prepend<T>(this IList<T> list, T element)
    {
        list.Insert(0, element);
    }

    /// <summary>Removes the first item in the list.</summary>
    public static void RemoveFirst(this IList list)
    {
        list.RemoveAt(0);
    }

    /// <summary>Removes the first item in the list machting the given predicate.</summary>
    public static void RemoveFirst<T>(this IList<T> list, Predicate<T> predicate)
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (predicate(list[i]))
            {
                list.RemoveAt(i);
                return;
            }
        }
        throw new InvalidOperationException("No item machting the given predicate.");
    }

    /// <summary>Tries to remove the first item in the list.</summary>
    public static bool TryRemoveFirst(this IList list)
    {
        if (list.Count > 0)
        {
            list.RemoveAt(0);
            return true;
        }
        return false;
    }

    /// <summary>Tries to remove the first item in the list machting the given predicate.</summary>
    public static bool TryRemoveFirst<T>(this IList<T> list, Predicate<T> predicate)
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

    /// <summary>Removes the last item in the list.</summary>
    public static void RemoveLast<T>(this IList<T> list)
    {
        list.RemoveAt(list.Count - 1);
    }

    /// <summary>Tries to remove the last item in the list.</summary>
    public static bool TryRemoveLast<T>(this IList<T> list)
    {
        if (list.Count > 0)
        {
            list.RemoveAt(list.Count - 1);
            return true;
        }
        return false;
    }

    /// <summary>Removes all elements from the list machting the given predicate.</summary>
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

    /// <summary>Returns the element at the specified index or the default value if the index is out of bounds.</summary>
    [return: MaybeNull]
    public static T GetOrDefault<T>(this IList<T> list, int index)
    {
        if (index > 0 && index < list.Count)
        {
            return list[index];
        }
        return default;
    }

    /// <summary>Retrieves the element at the specified index or the default value if the index is out of bounds. Returns whether the index is out of bounds.</summary>
    public static bool TryGet<T>(this IList<T> list, int index, [MaybeNullWhen(false)] out T value)
    {
        if (index > 0 && index < list.Count)
        {
            value = list[index];
            return true;
        }
        value = default!;
        return false;
    }

    /// <summary>Returns the predecessor of the given item or the deafault value if there is no predecessor.</summary>
    [return: MaybeNull]
    public static T GetPredecessor<T>(this IList<T> list, T item)
    {
        var index = list.IndexOf(item);
        if (index == -1)
        {
            throw new ArgumentException(nameof(item));
        }
        if (index - 1 >= 0)
        {
            return list[index - 1];
        }
        return default;
    }

    /// <summary>Returns the successor of the given item or the deafault value if there is no successor.</summary>
    [return: MaybeNull]
    public static T GetSuccessor<T>(this IList<T> list, T item)
    {
        var index = list.IndexOf(item);
        if (index == -1)
        {
            throw new ArgumentException(nameof(item));
        }
        if (index + 1 < list.Count)
        {
            return list[index + 1];
        }
        return default;
    }

    /// <summary>
    /// Replaces a element of the list with a new element.
    /// </summary>
    public static void Replace<T>(this IList<T> list, T oldElement, T newElement)
    {
        list[list.IndexOf(oldElement)] = newElement;
    }

    /// <summary>
    /// Returns a read-only wrapper for the current list.
    /// </summary>
    public static IReadOnlyList<T> AsReadonly<T>(this IList<T> list)
    {
        return new ReadOnlyCollection<T>(list);
    }

}