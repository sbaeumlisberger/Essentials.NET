namespace Essentials.NET;

public static class TimeProviderExtensions
{
    public static ITimer CreateTimer(this TimeProvider timeProvider, TimerCallback callback, TimeSpan dueTime)
    {
        return timeProvider.CreateTimer(callback, null, dueTime, Timeout.InfiniteTimeSpan);
    }

    public static ITimer CreatePerodicTimer(this TimeProvider timeProvider, TimerCallback callback, TimeSpan period)
    {
        return timeProvider.CreateTimer(callback, null, period, period);
    }
}
