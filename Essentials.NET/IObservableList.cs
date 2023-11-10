namespace Essentials.NET;

public interface IObservableList<T> : IObservableReadOnlyList<T>, IList<T>
{
    new int Count { get; }

    new T this[int index] { get; }

    new bool Contains(T value);

    new int IndexOf(T value);

    void Move(int oldIndex, int newIndex);

}
