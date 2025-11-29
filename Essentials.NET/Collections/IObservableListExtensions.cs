namespace Essentials.NET;

[Obsolete]
public static class IObservableListExtensions
{
    [Obsolete("Use SyncWith instead")]
    public static void MatchTo<T>(this IObservableList<T> list, IReadOnlyList<T> other)
    { 
        list.SyncWith(other);
    }
}