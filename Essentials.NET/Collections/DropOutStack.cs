using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Essentials.NET;

/// <summary>
/// An array-based stack with a fixed size. When an item is pushed to the stack 
/// and the size is reached, the bottom-most element of the stack is dropped.
/// </summary>
public class DropOutStack<T>(int size) : IReadOnlyCollection<T> where T : notnull
{
    public int Count => count;

    public T this[int index]
    {
        get => elements[(size + topIndex - index) % size];
    }

    private readonly T[] elements = new T[size];
    private int count = 0;
    private int topIndex = -1;

    public void Push(T element)
    {
        topIndex = (topIndex + 1) % size;
        elements[topIndex] = element;
        count = Math.Min(count + 1, size);
    }

    public T Pop()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }
        T element = elements[topIndex];
        elements[topIndex] = default!; // remove reference
        topIndex = (size + topIndex - 1) % size;
        count--;
        return element;
    }

    public bool TryPop([NotNullWhen(true)] out T? element)
    {
        if (Count > 0)
        {
            element = Pop();
            return true;
        }
        element = default;
        return false;
    }

    public T Peek()
    {
        if (Count == 0)
        {
            throw new InvalidOperationException("Stack is empty.");
        }
        return elements[topIndex];
    }

    public bool TryPeek([NotNullWhen(true)] out T? element)
    {
        if (Count > 0)
        {
            element = Peek();
            return true;
        }
        element = default;
        return false;
    }

    public void Clear()
    {
        count = 0;
        topIndex = -1;
        // remove references:
        for (int i = 0; i < elements.Length; i++)
        {
            elements[i] = default!;
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
            yield return this[i];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
