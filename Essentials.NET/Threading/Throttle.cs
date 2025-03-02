namespace Essentials.NET;

/// <summary>
/// Limits the execution of a function to a specific interval, even if the throttle is invoked much 
/// more frequently.
/// <br/><br/>
/// When the throttle is invoked, it executes the function and then ignores subsequent invocations 
/// until the execution has completed and a certain time span has elapsed. If invocations are made 
/// during this phase, the function is then executed again and the waiting period starts anew.
/// </summary>
public class Throttle : ThrottleBase<object?>
{
    /// <summary>
    /// Creates a throttle that executes the specifed function and uses the specified waiting period.
    /// </summary>
    /// <param name="waitingTime">The time to wait between executions of the function.</param>
    /// <param name="function">The function to be executed.</param>
    /// <param name="captureSynchronizationContext">Specifies whether the synchronization context 
    /// of the invocations should be captured and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan waitingTime, Func<Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(waitingTime, (_) => function(), captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Creates an object that throttles the invocation of a function.
    /// </summary>
    /// <param name="waitingTime">The time to wait between executions of the function.</param>
    /// <param name="function">The function to be executed.</param>
    /// <param name="captureSynchronizationContext">Specifies whether the synchronization context 
    /// of the invocations should be captured and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan waitingTime, Action function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    : base(waitingTime, (_) => { function(); return Task.CompletedTask; }, captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Invokes the throttle that limits the execution of the function specified during creation 
    /// to the configured interval.
    /// </summary>
    public void Invoke()
    {
        InvokeBase(null);
    }
}

/// <summary>
/// Limits the execution of a function to a specific interval, even if the throttle is invoked much 
/// more frequently.
/// <br/><br/>
/// When the throttle is invoked, it executes the function and then ignores subsequent invocations 
/// until the execution has completed and a certain time span has elapsed. If invocations are made 
/// during this phase, the function is then executed again and the waiting period starts anew.
/// </summary>
public class Throttle<T> : ThrottleBase<T>
{
    /// <summary>
    /// Creates an object that throttles the invocation of a function.
    /// </summary>
    /// <param name="waitingTime">The time to wait between executions of the function.</param>
    /// <param name="function">The function to be executed. It is always executed with the last input received.</param>
    /// <param name="captureSynchronizationContext">Specifies whether the synchronization context  
    /// of the invocations should be captured and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan waitingTime, Func<T, Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(waitingTime, function, captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Creates an object that throttles the invocation of a function.
    /// </summary>
    /// <param name="waitingTime">The time to wait between executions of the function.</param>
    /// <param name="function">The function to be executed. It is always executed with the last input received.</param>
    /// <param name="captureSynchronizationContext">Specifies whether the synchronization context 
    /// of the invocations should be captured and used for execution.</param>
    /// <param name="timeProvider"></param>
    public Throttle(TimeSpan waitingTime, Action<T> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
        : base(waitingTime, (value) => { function(value); return Task.CompletedTask; }, captureSynchronizationContext, timeProvider) { }

    /// <summary>
    /// Invokes the throttle that limits the execution of the function specified during creation 
    /// to the configured interval. The function is always executed with the last input received.
    /// </summary>
    /// <param name="input">The input that is passed to the function on execution.</param>
    public void Invoke(T input)
    {
        InvokeBase(input);
    }
}

public abstract class ThrottleBase<T> : IDisposable
{
    /// <summary>
    /// Inidcate whether execution of the function is pending.
    /// </summary>
    public bool IsExecutionPending => lastUnprocessedInput is not null;

    private readonly TimeSpan waitingTime;
    private readonly Func<T, Task> function;
    private readonly bool captureSynchronizationContext;
    private readonly TimeProvider timeProvider;

    private readonly object lockObject = new();

    private bool disposed;

    private bool canExecute = true;

    private ValueHolder? lastUnprocessedInput;

    private Task executionTask = Task.CompletedTask;

    private CancellationTokenSource waitingCancellationTokenSource = new();
    private CancellationToken waitingCancellationToken;

    internal protected ThrottleBase(TimeSpan waitingTime, Func<T, Task> function, bool captureSynchronizationContext = true, TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(waitingTime, TimeSpan.Zero, nameof(waitingTime));
        this.waitingTime = waitingTime;
        this.function = function;
        this.captureSynchronizationContext = captureSynchronizationContext;
        this.timeProvider = timeProvider ?? TimeProvider.System;
        waitingCancellationToken = waitingCancellationTokenSource.Token;
    }

    public void Dispose()
    {
        lock (lockObject)
        {
            if (!disposed)
            {
                disposed = true;
                waitingCancellationTokenSource.Cancel();
                waitingCancellationTokenSource.Dispose();
            }
        }
    }


    /// <summary>
    /// If execution is pending, the execution is started immediately after the last execution 
    /// has been completed without waiting for the configured time span to elapse.
    /// </summary>
    /// <returns>
    /// The task of the new execution or, if no execution was pending, the task of the last execution.
    /// </returns>
    public Task Flush()
    {
        lock (lockObject)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (lastUnprocessedInput is not null)
            {
                waitingCancellationTokenSource.Cancel();
                waitingCancellationTokenSource.Dispose();
                waitingCancellationTokenSource = new CancellationTokenSource();
                waitingCancellationToken = waitingCancellationTokenSource.Token;

                return executionTask.ContinueWith((_) =>
                {
                    lock (lockObject)
                    {
                        executionTask = function(lastUnprocessedInput);
                        lastUnprocessedInput = null;
                        executionTask.ContinueWith((_) => StartTimer(), GetTaskScheduler());
                        return executionTask;
                    }
                }, GetTaskScheduler()).Unwrap();
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
                executionTask = function(input);
                executionTask.ContinueWith((_) => StartTimer(), GetTaskScheduler());
            }
            else
            {
                lastUnprocessedInput = new ValueHolder(input);
            }
        }
    }

    private void StartTimer()
    {
        Task.Delay(waitingTime, timeProvider, waitingCancellationToken).ContinueWith(
              (_) => OnTimerElapsed(),
              waitingCancellationToken,
              TaskContinuationOptions.NotOnCanceled,
              GetTaskScheduler());
    }

    private void OnTimerElapsed()
    {
        lock (lockObject)
        {
            if (waitingCancellationToken.IsCancellationRequested)
            {
                return;
            }

            if (lastUnprocessedInput is not null)
            {
                executionTask = function(lastUnprocessedInput);
                lastUnprocessedInput = null;
                executionTask.ContinueWith((_) => StartTimer(), GetTaskScheduler());
            }
            else
            {
                canExecute = true;
            }
        }
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
