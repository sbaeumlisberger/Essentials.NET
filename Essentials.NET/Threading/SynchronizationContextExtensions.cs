using System.Runtime.ExceptionServices;

namespace Essentials.NET;

public static class SynchronizationContextExtensions
{
    public static void Dispatch(this SynchronizationContext synchronizationContext, Action action)
    {
        if (SynchronizationContext.Current == synchronizationContext)
        {
            action();
            return;
        }

        Exception? exception = null;

        synchronizationContext.Send(_ =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                exception = ex;
            }
        }, null);

        if (exception != null)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    public static Task DispatchAsync(this SynchronizationContext synchronizationContext, Action action)
    {
        if (SynchronizationContext.Current == synchronizationContext)
        {
            action();
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        synchronizationContext.Post(_ =>
        {
            try
            {
                action();
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);
        return tcs.Task;
    }

    public static Task DispatchAsync(this SynchronizationContext synchronizationContext, Func<Task> function)
    {
        if (SynchronizationContext.Current == synchronizationContext)
        {
            return function();
        }

        var tcs = new TaskCompletionSource();
        synchronizationContext.Post(async _ =>
        {
            try
            {
                await function().ConfigureAwait(false);
                tcs.SetResult();
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
        }, null);
        return tcs.Task;
    }

}
