namespace Essentials.NET;

public class Throttle
{
    private static readonly object DummyInput = new object();

    private readonly TimeSpan intervallTime;

    private readonly TimeProvider timeProvider;

    private readonly object lockObject = new object();

    private object? lastInput;

    public Throttle(TimeSpan intervallTime, TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(intervallTime, TimeSpan.Zero, nameof(intervallTime));
        this.intervallTime = intervallTime;
        this.timeProvider = timeProvider ?? TimeProvider.System;
    }

    public void Invoke<T>(T input, Func<T, Task> function) where T : notnull
    {
        lock (lockObject)
        {
            if (lastInput is null)
            {
                ExecuteAndWaitIntervallAsync(input, function);
            }
            lastInput = input;
        }
    }

    public void Invoke(Func<Task> function)
    {
        Invoke(DummyInput, _ => function());
    }

    private async void ExecuteAndWaitIntervallAsync<T>(T input, Func<T, Task> function)
    {
        long startTimeMillis = timeProvider.GetTimestamp();
        await function(input).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing | ConfigureAwaitOptions.ContinueOnCapturedContext);
        int executionTimeMillis = (int)timeProvider.GetElapsedTime(startTimeMillis).TotalMilliseconds;
        int intervallTimeMillis = (int)intervallTime.TotalMilliseconds;
        int waitTime = Math.Max(intervallTimeMillis - executionTimeMillis, 0);
        await Task.Delay(TimeSpan.FromMilliseconds(waitTime), timeProvider);

        lock (lockObject)
        {
            if (lastInput != null)
            {
                var _lastInput = (T)lastInput;
                lastInput = null;
                ExecuteAndWaitIntervallAsync(_lastInput, function);
            }
        }
    }
}
