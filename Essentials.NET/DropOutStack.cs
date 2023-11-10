using System.Collections;

namespace Essentials.NET;


/// <summary>
/// An array-based stack with a fixed capacity. When an item is pushed to the stack 
/// and the capacity is reached, the bottom-most element of the stack is dropped.
/// </summary>
public class DropOutStack<T> : IReadOnlyCollection<T>
{

    public int Count
    {
        get => _count;
        private set => _count = MathUtils.Clip(value, 0, items.Length);
    }

    private int TopIndex
    {
        get => _topIndex;
        set => _topIndex = GetIndexInBounds(value);
    }

    private readonly T[] items;
    private int _count = 0;
    private int _topIndex = -1;

    public DropOutStack(int capacity)
    {
        items = new T[capacity];
    }

    public void Push(T item)
    {
        TopIndex++;
        items[TopIndex] = item;
        Count++;
    }

    public T Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }
        T item = items[TopIndex];
        items[TopIndex] = default!; // remove reference
        TopIndex--;
        Count--;
        return item;
    }

    public T Peek()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }
        return items[TopIndex];
    }

    /// <summary>
    /// Gets an item from the stack. Index 0 is the last item pushed onto the stack.
    /// </summary>
    public T GetItem(int index)
    {
        if (index > Count)
        {
            throw new ArgumentOutOfRangeException("Index out of bounds.", nameof(index));
        }
        else
        {
            return items[GetIndexInBounds(TopIndex - index)];
        }
    }

    public void Clear()
    {
        Count = 0;
        // remove references:
        for (int i = 0; i < items.Length; i++)
        {
            items[i] = default!;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through the stack. The iterator 
    /// starts at the last item pushed onto the stack and goes backwards.
    /// </summary>
    public IEnumerator<T> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
        {
            yield return GetItem(i);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private int GetIndexInBounds(int index)
    {
        return (items.Length + index) % items.Length;
    }

}
