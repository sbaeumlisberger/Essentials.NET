namespace Essentials.NET;

/// <summary>
/// Throttles the invocation of a function. After the specified function is executed the function is
/// not executed again until the specified time interval has elapsed and the previous execution is completed.
/// </summary>
public class Throttle : ThrottleBase<object>
{
    private static readonly object DummyInput = new object();

    /// <summary>
    /// Creates an object that throttles the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval for which execution is paused after an invocation.</param>
    /// <param name="function">The function to be executed. May be cancelled if the execution takes longer than the interval time.</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan intervalTime, Func<CancellationToken, Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(intervalTime, (_, ct) => function(ct), captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Creates an object that throttles the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval for which execution is paused after an invocation.</param>
    /// <param name="function">The function to be executed.</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan intervalTime, Func<Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(intervalTime, (_, _) => function(), captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Creates an object that throttles the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval for which execution is paused after an invocation.</param>
    /// <param name="function">The function to be executed.</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan intervalTime, Action function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    : base(intervalTime, (_, _) => { function(); return Task.CompletedTask; }, captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Executes the function specified during creation as soon as the specified time interval since the last execution has elapsed.
    /// If it is the first invocation or the time interval has elapsed before, the function is executed immediately.
    /// If the previous execution is still running, it is cancelled and the new execution waits until completion.
    /// </summary>
    public void Invoke()
    {
        InvokeBase(DummyInput);
    }
}

/// <summary>
/// Throttles the invocation of a function. After the specified function is executed the function is
/// not executed again until the specified time interval has elapsed and the previous execution is completed.
/// </summary>
public class Throttle<T> : ThrottleBase<T> where T : notnull
{
    /// <summary>
    /// Creates an object that throttles the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval for which execution is paused after an invocation.</param>
    /// <param name="function">The function to be executed. May be cancelled if the execution takes longer than the interval time.</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan intervalTime, Func<T, CancellationToken, Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(intervalTime, function, captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Creates an object that throttles the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval for which execution is paused after an invocation.</param>
    /// <param name="function">The function to be executed.</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan intervalTime, Func<T, Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(intervalTime, (input, ct) => function(input), captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Creates an object that throttles the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval for which execution is paused after an invocation.</param>
    /// <param name="function">The function to be executed.</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan intervalTime, Action<T> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(intervalTime, (input, ct) => { function(input); return Task.CompletedTask; }, captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Executes the function specified during creation as soon as the specified time interval since the last execution has elapsed.
    /// If it is the first invocation or the time interval has elapsed before, the function is executed immediately.
    /// If the previous execution is still running, it is cancelled and the new execution waits until completion.
    /// The function is always executed with the last received input.
    /// </summary>
    /// <param name="input">The input that is passed to the function on execution</param>
    public void Invoke(T input)
    {
        InvokeBase(input);
    }
}
public abstract class ThrottleBase<T> : IDisposable where T : notnull
{
    private readonly TimeSpan intervalTime;
    private readonly Func<T, CancellationToken, Task> function;
    private readonly bool captureSynchronizationContext = true;
    private readonly TimeProvider timeProvider;

    private readonly object lockObject = new object();

    private bool canExecute = true;

    private T? next;

    private CancellationTokenSource? cancellationTokenSource;

    private Task executionTask = Task.CompletedTask;

    private bool disposed;

    internal protected ThrottleBase(TimeSpan intervalTime, Func<T, CancellationToken, Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(intervalTime, TimeSpan.Zero, nameof(intervalTime));
        this.intervalTime = intervalTime;
        this.function = function;
        this.captureSynchronizationContext = captureSynchronizationContext;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public void Dispose()
    {
        lock (lockObject)
        {
            disposed = true;
            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            next = default;
        }
    }

    /// <summary>
    /// Resets the throttle. If there is an active execution, it is cancelled.
    /// </summary>
    public void Reset()
    {
        lock (lockObject)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            next = default;
            canExecute = true;
        }
    }

    /// <summary>
    /// When execution is pending, it is started immediately and the active execution is cancelled.
    /// The new execution waits until the previous execution has ended.
    /// </summary>
    /// <returns>
    /// The task of the started execution or, if execution was not pending, the task of the last started execution.
    /// </returns>
    public Task Flush()
    {
        lock (lockObject)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (next is not null)
            {
                var input = next;

                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                next = default;
                canExecute = true;

                cancellationTokenSource = new CancellationTokenSource();
                executionTask = ExecuteAndStartTimer(input, cancellationTokenSource.Token);
            }
            return executionTask;
        }
    }

    protected void InvokeBase(T input)
    {
        lock (lockObject)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (canExecute)
            {
                canExecute = false;
                cancellationTokenSource = new CancellationTokenSource();
                executionTask = ExecuteAndStartTimer(input, cancellationTokenSource.Token);
            }
            else
            {
                next = input;
            }
        }
    }

    private Task ExecuteAndStartTimer(T input, CancellationToken cancellationToken)
    {
        return executionTask.ContinueWith(_ =>
        {
            Task newExecutionTask;

            lock (lockObject)
            {
                cancellationToken.ThrowIfCancellationRequested();
                newExecutionTask = function(input, cancellationToken);
            }

            StartTimer(cancellationToken);

            return newExecutionTask;

        }, cancellationToken, TaskContinuationOptions.None, captureSynchronizationContext).Unwrap();
    }

    private void StartTimer(CancellationToken cancellationToken)
    {
        Task.Delay(intervalTime, timeProvider, cancellationToken).ContinueWith(
            _ => OnTimerElapsed(cancellationToken),
            cancellationToken,
            TaskContinuationOptions.NotOnCanceled,
            captureSynchronizationContext);
    }

    private void OnTimerElapsed(CancellationToken cancellationToken)
    {
        lock (lockObject)
        {
            cancellationToken.ThrowIfCancellationRequested();

            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;

            if (next is not null)
            {
                var input = next;
                next = default;
                cancellationTokenSource = new CancellationTokenSource();
                executionTask = ExecuteAndStartTimer(input, cancellationTokenSource.Token);
            }
            else
            {
                canExecute = true;
            }
        }
    }
}
