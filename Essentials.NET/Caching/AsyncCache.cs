using System.Collections.Concurrent;

namespace Essentials.NET;

public class AsyncCache<TKey, TValue> where TKey : notnull
{
    public int CurrentSize => cache.CurrentSize;

    public int MaxSize => cache.MaxSize;

    private readonly Cache<TKey, CacheableTask<TValue>> cache;

    private readonly Action<TValue>? removedCallback;

    private readonly object lockObject = new();

    public AsyncCache(int maxSize, Action<TValue>? removedCallback = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSize, 1);
        cache = new Cache<TKey, CacheableTask<TValue>>(maxSize, InvokeRemovedCallback);
        this.removedCallback = removedCallback;
    }

    public void Remove(TKey key)
    {
        cache.Remove(key);
    }

    public Task<TValue> GetOrCreateAsync(TKey key, Func<TKey, CancellationToken, Task<TValue>> createValueCallback, CancellationToken cancellationToken = default)
    {
        lock (lockObject)
        {
            var cacheableTask = cache.GetOrCreate(key, key =>
            {
                return new CacheableTask<TValue>(token => createValueCallback(key, token), () => cache.Remove(key), lockObject);
            });
            return cacheableTask.GetTask(cancellationToken);
        }
    }

    private void InvokeRemovedCallback(CacheableTask<TValue> cacheableTask)
    {
        cacheableTask.InternalTask.ContinueWith(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                removedCallback?.Invoke(task.Result);
            }
            cacheableTask.Dispose();
        });
    }

    private class CacheableTask<T> : IDisposable
    {
        internal Task<T> InternalTask => internalTask;

        private readonly Task<T> internalTask;

        private readonly Action cancellationCallback;

        private readonly object lockObject;

        private readonly CancellationTokenSource cancellationTokenSource = new();

        private readonly ConcurrentBag<CancellationTokenRegistration> cancellationTokenRegistrations = new();

        private int cancellationTokensCount = 0;

        private bool isDisposed = false;

        public CacheableTask(Func<CancellationToken, Task<T>> createTask, Action cancellationCallback, object lockObject)
        {
            internalTask = createTask(cancellationTokenSource.Token);
            this.cancellationCallback = cancellationCallback;
            this.lockObject = lockObject;
        }

        public Task<T> GetTask(CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);

            if (internalTask.IsCompleted)
            {
                return internalTask;
            }

            var tsc = new TaskCompletionSource<T>();

            cancellationTokensCount++;

            cancellationTokenRegistrations.Add(cancellationToken.Register(() =>
            {
                lock (lockObject)
                {
                    cancellationTokensCount--;
                    if (cancellationTokensCount == 0)
                    {
                        cancellationTokenSource.Cancel();
                        cancellationCallback();
                    }
                    tsc.TrySetCanceled();
                }
            }));

            internalTask.ContinueWith(task =>
            {
                if (task.IsCompletedSuccessfully)
                {
                    tsc.TrySetResult(task.Result);
                }
                else if (task.IsCanceled)
                {
                    tsc.TrySetCanceled();
                }
                else
                {
                    tsc.TrySetException(task.Exception!);
                }
            });

            return tsc.Task;
        }

        public void Dispose()
        {
            isDisposed = true;
            cancellationTokenRegistrations.ForEach(registration => registration.Dispose());
            cancellationTokenSource.Dispose();
        }
    }
}
