namespace Essentials.NET;

public static class TimeProviderExtensions
{
    public static ITimer CreateTimer(this TimeProvider timeProvider, TimeSpan dueTime, Action callback)
    {
        return timeProvider.CreateTimer(_ => callback(), null, dueTime, Timeout.InfiniteTimeSpan);
    }

    public static ITimer CreatePerodicTimer(this TimeProvider timeProvider, TimeSpan period, Action callback)
    {
        return timeProvider.CreateTimer(_ => callback(), null, period, period);
    }
}
