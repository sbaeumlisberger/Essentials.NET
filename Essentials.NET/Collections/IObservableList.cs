namespace Essentials.NET;

public interface IObservableList<T> : IObservableReadOnlyList<T>, IList<T>
{
    new int Count { get; }

    new T this[int index] { get; }

    new bool Contains(T value);

    new int IndexOf(T value);

    void Move(int oldIndex, int newIndex);

    void SyncWith(IReadOnlyList<T> other)
    {
        this.RemoveRange(this.Except(other).ToList());

        for (int i = 0; i < other.Count; i++)
        {
            if (i > this.Count - 1)
            {
                this.Add(other[i]);
            }
            else if (!Equals(other[i], this[i]))
            {
                int oldIndex = this.IndexOf(other[i]);
                if (oldIndex != -1)
                {
                    this.Move(oldIndex, i);
                }
                else
                {
                    this.Insert(i, other[i]);
                }
            }
        }
    }
}
