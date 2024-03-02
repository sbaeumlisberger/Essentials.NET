namespace Essentials.NET;

public static class TaskExtensions
{
    public static Task ContinueWith(
        this Task task,
        Action<Task> continuationAction,
        CancellationToken cancellationToken = default,
        TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
        bool continueOnCurrentSynchronizationContext = false)
    {
        var taskScheduler = continueOnCurrentSynchronizationContext && SynchronizationContext.Current is not null
            ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Current;
        return task.ContinueWith(continuationAction, cancellationToken, continuationOptions, taskScheduler);
    }

    public static Task<TResult> ContinueWith<TResult>(
        this Task task,
        Func<Task, TResult> continuationAction,
        CancellationToken cancellationToken = default,
        TaskContinuationOptions continuationOptions = TaskContinuationOptions.None,
        bool continueOnCurrentSynchronizationContext = false)
    {
        var taskScheduler = continueOnCurrentSynchronizationContext && SynchronizationContext.Current is not null
            ? TaskScheduler.FromCurrentSynchronizationContext() : TaskScheduler.Current;
        return task.ContinueWith(continuationAction, cancellationToken, continuationOptions, taskScheduler);
    }
}
