namespace Essentials.NET;

/// <summary>
/// Represents a disposable object that can be used by multiple consumers.
/// </summary>
public interface IShareableDisposable : IDisposable
{
    /// <summary>
    /// Increments the usage count of the shared resource.
    /// </summary>
    void IncrementUsageCount();

    /// <summary>
    /// Decrements the usage count of the shared resource.
    /// If the usage count reaches zero, the object is disposed.
    /// </summary>
    new void Dispose();
}

public static class ShareableDisposableExtensions
{
    /// <summary>
    /// Increments the usage count of the shared resource and returns itself.
    /// </summary>
    public static T RequestUsage<T>(this T shareableDisposables) where T : IShareableDisposable
    {
        shareableDisposables.IncrementUsageCount();
        return shareableDisposables;
    }
}

/// <summary>
/// Base class for disposable objects that can be used by multiple consumers.
/// </summary>
public abstract class ShareableDisposable : IShareableDisposable
{
    private readonly object lockObject = new();

    private int usageCount = 1;

    private bool isDisposed = false;

    public void IncrementUsageCount()
    {
        lock (lockObject)
        {
            ThrowIfDisposed();
            usageCount++;
        }
    }

    public void Dispose()
    {
        lock (lockObject)
        {
            usageCount--;

            if (usageCount == 0)
            {
                isDisposed = true;
                GC.SuppressFinalize(this);
                OnDispose();
            }
        }
    }

    /// <summary>
    /// Called when the object is no longer used by any consumer and has to be disposed.
    /// </summary>
    protected abstract void OnDispose();

    /// <summary>
    /// Throws an <see cref="ObjectDisposedException"/> if the object has been disposed.
    /// </summary>
    /// <exception cref="ObjectDisposedException"/>"
    protected void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
    }
}