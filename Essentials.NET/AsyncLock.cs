namespace Essentials.NET;

public class AsyncLock
{
    private Task waitTask = Task.CompletedTask;

    public Task<IDisposable> AcquireAsync()
    {
        lock (this)
        {
            var lockTSC = new TaskCompletionSource<bool>();
            var acquireTask = waitTask.IsCompleted
               ? Task.FromResult<IDisposable>(new Disposable(() => lockTSC.SetResult(true)))
               : waitTask.ContinueWith<IDisposable>(_ => new Disposable(() => lockTSC.SetResult(true)));
            waitTask = lockTSC.Task;
            return acquireTask;
        }
    }

}
