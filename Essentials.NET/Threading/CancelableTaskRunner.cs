namespace Essentials.NET;

public class CancelableTaskRunner
{
    private readonly object lockObject = new();

    private CancellationTokenSource? currentCancellationTokenSource = null;

    private Task? currentTask;

    public Task RunAndCancelPrevious(Func<CancellationToken, Task> asyncFunction)
    {
        currentCancellationTokenSource?.Cancel();

        var cancellationTokenSource = new CancellationTokenSource();

        lock (lockObject)
        {
            currentCancellationTokenSource = cancellationTokenSource;

            currentTask = WaitThanExecute(currentTask, asyncFunction, cancellationTokenSource);

            return currentTask;
        }
    }

    private async Task WaitThanExecute(Task? previousTask, Func<CancellationToken, Task> asyncFunction, CancellationTokenSource cancellationTokenSource)
    {
        if (previousTask is not null)
        {
            await previousTask.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
        }

        try
        {
            await asyncFunction(cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // prevent cancellation exceptions from bubbeling up
        }
        finally
        {
            lock (lockObject)
            {
                if (cancellationTokenSource == currentCancellationTokenSource)
                {
                    currentCancellationTokenSource = null;
                    currentTask = null;
                }
            }

            cancellationTokenSource.Dispose();
        }
    }

    public void Cancel()
    {
        currentCancellationTokenSource?.Cancel();
    }
}
