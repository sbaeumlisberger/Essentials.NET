namespace Essentials.NET;

/// <summary>
/// A mutual exclusion lock that works with asynchronous code (async/wait).
/// <br/><br/> Usage:
/// <code>
/// private readonly AsyncLock asyncLock = new AsyncLock();
/// 
/// using (await asyncLock.AcquireAsync())
/// {
///     // asynchronous code e.g. await Task.Delay(100);
/// }
/// </code>
/// </summary>
public class AsyncLock
{
    private Task lockAvailableTask = Task.CompletedTask;

    private readonly object lockObject = new object();

    /// <summary>
    /// Acquires the lock asynchronously.
    /// </summary>
    /// <returns>A disposable that releases the lock when disposed.</returns>
    public Task<IDisposable> AcquireAsync()
    {
        lock (lockObject)
        {
            var releasedTCS = new TaskCompletionSource();
            var releasingDisposable = new DelegatingDisposable(releasedTCS.SetResult);
            var aquireTask = lockAvailableTask.IsCompleted
                ? Task.FromResult<IDisposable>(releasingDisposable)
                : lockAvailableTask.ContinueWith<IDisposable>(_ => releasingDisposable);
            lockAvailableTask = releasedTCS.Task;
            return aquireTask;
        }
    }

}
