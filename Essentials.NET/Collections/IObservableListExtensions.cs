namespace Essentials.NET;

public static class IObservableListExtensions
{
    [Obsolete("Use SyncWith instead")]
    public static void MatchTo<T>(this IObservableList<T> list, IReadOnlyList<T> other)
    { 
        SyncWith(list, other);
    }

    public static void SyncWith<T>(this IObservableList<T> list, IReadOnlyList<T> other)
    {
        list.RemoveRange(list.Except(other).ToList());

        for (int i = 0; i < other.Count; i++)
        {
            if (i > list.Count - 1)
            {
                list.Add(other[i]);
            }
            else if (!Equals(other[i], list[i]))
            {
                int oldIndex = list.IndexOf(other[i]);
                if (oldIndex != -1)
                {
                    list.Move(oldIndex, i);
                }
                else
                {
                    list.Insert(i, other[i]);
                }
            }
        }
    }

}