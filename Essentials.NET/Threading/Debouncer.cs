namespace Essentials.NET;

/// <summary>
/// Executes a function after the debouncer has been invoked and a certain time span has elapsed without another invocation.
/// </summary>
public class Debouncer : DebouncerBase<object?>
{
    /// <summary>
    /// Creates a debouncer that executes the specified function and uses the specified debounce time.
    /// </summary>
    /// <param name="debounceTime">The time span that must elapse without another invocation before the function is executed.</param>
    /// <param name="function">The function to be executed.</param>
    /// <param name="captureSynchronizationContext">Specifies whether the synchronization context of the invocations should be capurted and used for execution</param>
    /// <param name="timeProvider"></param>
    public Debouncer(TimeSpan debounceTime, Func<Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    : base(debounceTime, (_) => function(), captureSynchronizationContext, timeProvider)
    {
    }

    /// <summary>
    /// Creates a debouncer that executes the specified function and uses the specified debounce time.
    /// </summary>
    /// <param name="debounceTime">The time span that must elapse without another invocation before the function is executed.</param>
    /// <param name="function">The function to be executed.</param>
    /// <param name="captureSynchronizationContext">Specifies whether the synchronization context of the invocations should be capurted and used for execution</param>
    /// <param name="timeProvider"></param>
    public Debouncer(TimeSpan debounceTime, Action function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(debounceTime, (_) => function(), captureSynchronizationContext, timeProvider)
    {
    }

    /// <summary>
    /// Invokes the debouncer. The function specified on creation is executed after the configured time span
    /// has elapsed without another invocation.
    /// </summary>
    public void Invoke()
    {
        InvokeBase(null);
    }
}

/// <summary>
/// Executes a function after the debouncer has been invoked and a certain time span has elapsed without another invocation.
/// </summary>
public class Debouncer<T> : DebouncerBase<T>
{
    /// <summary>
    /// Creates a debouncer that executes the specified function and uses the specified debounce time.
    /// </summary>
    /// <param name="debounceTime">The time span that must elapse without another invocation before the function is executed.</param>
    /// <param name="function">The function to be executed. It is always invoked with the last input received.</param>
    /// <param name="captureSynchronizationContext">Specifies whether the synchronization context of the invocations should be capurted and used for execution</param>
    /// <param name="timeProvider"></param>
    public Debouncer(TimeSpan debounceTime, Func<T, Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    : base(debounceTime, value => function(value), captureSynchronizationContext, timeProvider)
    {
    }

    /// <summary>
    /// Creates a debouncer that executes the specified function and uses the specified debounce time.
    /// </summary>
    /// <param name="debounceTime">The time span that must elapse without another invocation before the function is executed.</param>
    /// <param name="function">The function to be executed. It is always invoked with the last input received.</param>
    /// <param name="captureSynchronizationContext">Specifies whether the synchronization context of the invocations should be capurted and used for execution</param>
    /// <param name="timeProvider"></param>
    public Debouncer(TimeSpan debounceTime, Action<T> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    : base(debounceTime, function, captureSynchronizationContext, timeProvider)
    {
    }

    /// <summary>
    /// Invokes the debouncer. The function specified on creation is executed after the configured time span 
    /// has elapsed without another invocation. The function is always executed with the last input received.
    /// </summary>
    /// <param name="input">The input that is passed to the function on execution.</param>
    public void Invoke(T input)
    {
        InvokeBase(input);
    }
}

public abstract class DebouncerBase<T> : IDisposable
{
    /// <summary>
    /// Inidcate whether execution of the function is pending.
    /// </summary>
    public bool IsExecutionPending => lastUnprocessedInput is not null;

    private readonly TimeSpan debounceTime;
    private readonly Action<T> function;
    private readonly bool captureSynchronizationContext;
    private readonly TimeProvider timeProvider;

    private readonly object lockObject = new();

    private bool disposed;

    private ValueHolder? lastUnprocessedInput;

    private long lastInvocationTimestamp;

    private CancellationTokenSource waitingCancellationTokenSource = new();

    internal protected DebouncerBase(TimeSpan debounceTime, Action<T> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(debounceTime, TimeSpan.Zero, nameof(debounceTime));
        this.debounceTime = debounceTime;
        this.function = function;
        this.captureSynchronizationContext = captureSynchronizationContext;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public void Dispose()
    {
        lock (lockObject)
        {
            disposed = true;
            waitingCancellationTokenSource.Cancel();
            waitingCancellationTokenSource.Dispose();
        }
    }

    /// <summary>
    /// If execution is pending, it is started immediately without waiting for the configured time span to elapse.
    /// </summary>
    public void Flush()
    {
        lock (lockObject)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (lastUnprocessedInput is not null)
            {
                waitingCancellationTokenSource.Cancel();
                waitingCancellationTokenSource.Dispose();
                waitingCancellationTokenSource = new CancellationTokenSource();
                Execute();
            }
        }
    }

    protected void InvokeBase(T input)
    {
        lock (lockObject)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            bool startDebounce = lastUnprocessedInput is null;

            lastUnprocessedInput = new ValueHolder(input);
            lastInvocationTimestamp = timeProvider.GetTimestamp();
            
            if (startDebounce)
            {
                WaitThenExecute(waitingCancellationTokenSource.Token);
            }
        }
    }

    private void WaitThenExecute(CancellationToken cancellationToken)
    {
        lock (lockObject)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            var elapsedTimeSinceLastInvocation = timeProvider.GetElapsedTime(lastInvocationTimestamp);
            var waitTime = debounceTime - elapsedTimeSinceLastInvocation;

            if (waitTime > TimeSpan.Zero)
            {
                Task.Delay(waitTime, timeProvider, cancellationToken).ContinueWith(
                    _ => WaitThenExecute(cancellationToken),
                    cancellationToken,
                    TaskContinuationOptions.NotOnCanceled,
                    GetTaskScheduler());
            }
            else
            {
                Execute();
            }
        }
    }

    private void Execute()
    {
        var input = lastUnprocessedInput!;
        lastUnprocessedInput = null;
        function(input);
    }

    private TaskScheduler GetTaskScheduler()
    {
        return captureSynchronizationContext && SynchronizationContext.Current is not null
                ? TaskScheduler.FromCurrentSynchronizationContext()
                : TaskScheduler.Current;
    }

    private record ValueHolder(T Value)
    {
        public static implicit operator T(ValueHolder valueHolder) => valueHolder.Value;
    }
}
