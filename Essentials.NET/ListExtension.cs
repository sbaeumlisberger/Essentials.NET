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

    /// <summary>Returns the predecessor of the given item or the deafault value if there is no predecessor.</summary>
    [return: MaybeNull]
    public static T GetPredecessor<T>(this IList<T> list, T item)
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
    public static T GetPredecessor<T>(this List<T> list, T item)
    {
        return GetPredecessor((IList<T>)list, item);
    }

    /// <summary>Returns the successor of the given item or the deafault value if there is no successor.</summary>
    [return: MaybeNull]
    public static T GetSuccessor<T>(this IList<T> list, T item)
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

    /// <summary>Returns the successor of the given item or the deafault value if there is no successor.</summary>
    [return: MaybeNull]
    public static T GetSuccessor<T>(this List<T> list, T item)
    {
        return GetSuccessor((IList<T>)list, item);
    }

    /// <summary>
    /// Replaces a element of the list with a new element.
    /// </summary>
    public static void Replace<T>(this IList<T> list, T oldElement, T newElement)
    {
        int index = list.IndexOf(oldElement);
        if(index == -1) 
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