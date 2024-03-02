namespace Essentials.NET;

public static class ITimerExtensions
{
    /// <summary>
    /// Restarts the specified timer.
    /// </summary> 
    /// <param name="dueTime">A <see cref="System.TimeSpan"/> representing the amount of time to delay before
    /// invoking the callback method specified when the timer was constructed.</param>  
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    public static bool Restart(this ITimer timer, TimeSpan dueTime)
    {
        return timer.Change(dueTime, Timeout.InfiniteTimeSpan);
    }

    /// <summary>
    /// Restarts the specified timer.
    /// </summary>
    /// <param name="period">A <see cref="System.TimeSpan"/> representing the time interval between invocations 
    /// of the callback method specified when the timer was constructed. Specify <see cref="Timeout.InfiniteTimeSpan"/>
    /// to disable periodic signaling.
    /// </param>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    public static bool RestartPeriodic(this ITimer timer, TimeSpan period)
    {
        return timer.Change(period, period);
    }

    /// <summary>
    /// Stops the specified timer.
    /// </summary>
    /// <returns>true if the timer was successfully updated; otherwise, false.</returns>
    public static bool Stop(this ITimer timer)
    {
        return timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }
}
