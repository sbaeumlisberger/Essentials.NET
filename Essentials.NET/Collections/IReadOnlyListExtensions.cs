namespace Essentials.NET;

public static class IReadOnlyListExtensions
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
}
