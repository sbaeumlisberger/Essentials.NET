namespace Essentials.NET;

public class CancelableTaskRunner
{
    private readonly bool asCompletedOnCancelation;

    private readonly object lockObject = new();

    private CancellationTokenSource? currentCancellationTokenSource = null;

    public CancelableTaskRunner(bool asCompletedOnCancelation = true)
    {
        this.asCompletedOnCancelation = asCompletedOnCancelation;
    }

    public Task RunAndCancelPrevious(Func<CancellationToken, Task> asyncFunction)
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        lock (lockObject)
        {
            currentCancellationTokenSource?.Cancel();
            currentCancellationTokenSource = cancellationTokenSource;
        }

        return asyncFunction(cancellationToken).ContinueWith(task =>
        {
            lock (lockObject)
            {
                if (cancellationTokenSource == currentCancellationTokenSource)
                {
                    currentCancellationTokenSource = null;
                }
            }

            cancellationTokenSource.Dispose();

            if (task.IsCanceled && asCompletedOnCancelation)
            {
                return Task.CompletedTask;
            }
            return task;

        }).Unwrap();
    }

    public void Cancel()
    {
        currentCancellationTokenSource?.Cancel();
    }
}
