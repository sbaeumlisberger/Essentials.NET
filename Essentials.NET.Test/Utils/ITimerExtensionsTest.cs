using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Essentials.NET.Test.Utils;

public class ITimerExtensionsTest
{
    private readonly FakeTimeProvider timeProvider = new FakeTimeProvider();

    [Fact]
    public void Restart_AfterInvocation()
    {
        int invocationCount = 0;

        Action callback = () => invocationCount++;

        var timer = timeProvider.CreateTimer(TimeSpan.FromMilliseconds(200), callback);

        timeProvider.Advance(TimeSpan.FromMilliseconds(250));

        Assert.Equal(1, invocationCount);

        Assert.True(timer.Restart(TimeSpan.FromMilliseconds(200)));

        timeProvider.Advance(TimeSpan.FromMilliseconds(200));

        Assert.Equal(2, invocationCount);

        timeProvider.Advance(TimeSpan.FromMilliseconds(200));

        Assert.Equal(2, invocationCount);
    }

    [Fact]
    public void Restart_BeforeInvocation()
    {
        int invocationCount = 0;

        Action callback = () => invocationCount++;

        var timer = timeProvider.CreateTimer(TimeSpan.FromMilliseconds(200), callback);

        timeProvider.Advance(TimeSpan.FromMilliseconds(100));

        Assert.Equal(0, invocationCount);

        Assert.True(timer.Restart(TimeSpan.FromMilliseconds(200)));

        timeProvider.Advance(TimeSpan.FromMilliseconds(100));

        Assert.Equal(0, invocationCount);

        timeProvider.Advance(TimeSpan.FromMilliseconds(100));

        Assert.Equal(1, invocationCount);

        timeProvider.Advance(TimeSpan.FromMilliseconds(200));

        Assert.Equal(1, invocationCount);
    }

    [Fact]
    public void Stop()
    {
        int invocationCount = 0;

        Action callback = () => invocationCount++;

        var timer = timeProvider.CreateTimer(TimeSpan.FromMilliseconds(200), callback);

        timeProvider.Advance(TimeSpan.FromMilliseconds(100));

        Assert.Equal(0, invocationCount);

        Assert.True(timer.Stop());

        timeProvider.Advance(TimeSpan.FromMilliseconds(100));

        Assert.Equal(0, invocationCount);

        timeProvider.Advance(TimeSpan.FromMilliseconds(200));

        Assert.Equal(0, invocationCount);
    }

}
