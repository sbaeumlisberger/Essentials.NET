namespace Essentials.NET;

/// <summary>
/// Debounces the invocation of a function. The specified function is executed 
/// whenever the specified time interval has elapsed since the last invocation.
/// </summary>
public class Debouncer : DebouncerBase<object>
{
    private static readonly object DummyInput = new object();

    /// <summary>
    /// Creates an object that debounces the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval without a invocation before execution takes place</param>
    /// <param name="function">The function to be executed</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution</param>
    /// <param name="timeProvider"></param>
    public Debouncer(TimeSpan intervalTime, Func<Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    : base(intervalTime, _ => function(), captureSynchronizationContext, timeProvider)
    {
    }

    /// <summary>
    /// Creates an object that debounces the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval without a invocation before execution takes place</param>
    /// <param name="function">The function to be executed</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution</param>
    /// <param name="timeProvider"></param>
    public Debouncer(TimeSpan intervalTime, Action function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(intervalTime, _ => { function(); return Task.CompletedTask; }, captureSynchronizationContext, timeProvider)
    {
    }

    /// <summary>
    /// On the initial call and every first call since the last execution, debouncing is started. 
    /// If debouncing is already in progress, the time interval is reset.
    /// When the time interval elapses without further invocations, the function specified during creation is executed.
    /// </summary>
    public void Invoke()
    {
        InvokeBase(DummyInput);
    }
}

/// <summary>
/// Debounces the invocation of a function. The specified function is executed 
/// whenever the specified time interval has elapsed since the last invocation.
/// </summary>
public class Debouncer<T> : DebouncerBase<T> where T : notnull
{
    /// <summary>
    /// Creates an object that debounces the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval without a invocation before execution takes place</param>
    /// <param name="function">The function to be executed</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution</param>
    /// <param name="timeProvider"></param>
    public Debouncer(TimeSpan intervalTime, Func<T, Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    : base(intervalTime, function, captureSynchronizationContext, timeProvider)
    {
    }

    /// <summary>
    /// Creates an object that debounces the invocation of a function.
    /// </summary>
    /// <param name="intervalTime">The time interval without a invocation before execution takes place</param>
    /// <param name="function">The function to be executed</param>
    /// <param name="captureSynchronizationContext">Specifies if the synchronization context of the invocations should be capurted and used for execution</param>
    /// <param name="timeProvider"></param>
    public Debouncer(TimeSpan intervalTime, Action<T> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    : base(intervalTime, input => { function(input); return Task.CompletedTask; }, captureSynchronizationContext, timeProvider)
    {
    }

    /// <summary>
    /// On the initial call and every first call since the last execution, debouncing is started. 
    /// If debouncing is already in progress, the time interval is reset. 
    /// When the time interval elapses without further invocations, 
    /// the function specified during creation is executed with the last received input.
    /// </summary>
    /// <param name="input">The input that is passed to the function on execution</param>
    public void Invoke(T input)
    {
        InvokeBase(input);
    }
}

public abstract class DebouncerBase<T> : IDisposable where T : notnull
{
    private readonly TimeSpan intervalTime;

    private readonly Func<T, Task> function;

    private readonly bool captureSynchronizationContext = true;

    private readonly TimeProvider timeProvider;

    private readonly object lockObject = new object();

    private bool disposed = false;

    private bool isWaiting = false;

    private T? lastInput;

    private long lastInvocationTime;

    private Task executionTask = Task.CompletedTask;

    private CancellationTokenSource? cancellationTokenSource;

    internal protected DebouncerBase(TimeSpan intervalTime, Func<T, Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
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
        }
    }

    /// <summary>
    /// When execution is pending, it is started immediately without waiting for the time interval to elapse.
    /// </summary>
    /// <returns>
    /// The task of the execution or, if execution was not pending, <see cref="Task.CompletedTask"/>.
    /// </returns>
    public Task Flush()
    {
        lock (lockObject)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (lastInput is not null)
            {
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
                isWaiting = false;
                var input = lastInput;
                lastInput = default;
                lastInvocationTime = 0;
                executionTask = function(input);
                return executionTask;
            }
            return executionTask;
        }
    }

    protected void InvokeBase(T input)
    {
        lock (lockObject)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            lastInput = input;
            lastInvocationTime = timeProvider.GetTimestamp();

            if (!isWaiting)
            {
                isWaiting = true;
                cancellationTokenSource = new CancellationTokenSource();
                WaitAndThanExecute(cancellationTokenSource.Token);
            }
        }
    }

    private void WaitAndThanExecute(CancellationToken cancellationToken)
    {
        lock (lockObject)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var elapsedTimeSinceLastInvocation = timeProvider.GetTimestamp() - lastInvocationTime;
            var waitTime = TimeSpan.FromTicks(intervalTime.Ticks - elapsedTimeSinceLastInvocation);

            if (waitTime <= TimeSpan.Zero)
            {
                isWaiting = false;
                var input = lastInput!;
                lastInput = default;
                lastInvocationTime = 0;
                executionTask = function(input);
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
            else
            {
                Task.Delay(waitTime, timeProvider, cancellationToken).ContinueWith(
                    _ => WaitAndThanExecute(cancellationToken),
                    cancellationToken,
                    TaskContinuationOptions.NotOnCanceled,
                    captureSynchronizationContext);
            }
        }
    }
}
