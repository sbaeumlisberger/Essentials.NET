using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Essentials.NET.Test;

public class ThrottleTest
{
    [Fact]
    public async Task Throttle()
    {
        var testSynchronizationContext = SynchronizationContext.Current;
        Assert.NotNull(testSynchronizationContext);

        var timeProvider = new FakeTimeProvider();
        var intervallTime = TimeSpan.FromMilliseconds(100);

        var throttle = new Throttle(intervallTime, timeProvider);

        int functionInvocationCount = 0;
        SynchronizationContext? functionSynchronizationContext = null;

        var function = async () =>
        {
            functionInvocationCount++;
            functionSynchronizationContext = SynchronizationContext.Current;
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        throttle.Invoke(function);

        Assert.Equal(1, functionInvocationCount);
        Assert.Equal(testSynchronizationContext, functionSynchronizationContext);

        throttle.Invoke(function);
        throttle.Invoke(function);
        throttle.Invoke(function);

        Assert.Equal(1, functionInvocationCount);

        await Task.Run(() => timeProvider.Advance(intervallTime / 2));
        await Task.Yield();

        Assert.Equal(1, functionInvocationCount);

        await Task.Run(() => timeProvider.Advance(intervallTime / 2));
        await Task.Yield();

        Assert.Equal(2, functionInvocationCount);
        Assert.Equal(testSynchronizationContext, functionSynchronizationContext);

        await Task.Run(() => timeProvider.Advance(intervallTime));
        await Task.Yield();

        Assert.Equal(2, functionInvocationCount);

        throttle.Invoke(function);

        Assert.Equal(3, functionInvocationCount);
        Assert.Equal(testSynchronizationContext, functionSynchronizationContext);
    }

    [Fact]
    public void Throttle_WithInput()
    {
        var timeProvider = new FakeTimeProvider();
        var intervallTime = TimeSpan.FromMilliseconds(100);

        var throttle = new Throttle(intervallTime, timeProvider);

        int functionInvocationCount = 0;
        string? lastValue = null;

        var function = async (string value) =>
        {
            functionInvocationCount++;
            lastValue = value;
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        throttle.Invoke("Value 1", function);

        Assert.Equal(1, functionInvocationCount);
        Assert.Equal("Value 1", lastValue);

        throttle.Invoke("Value 2", function);
        throttle.Invoke("Value 3", function);
        throttle.Invoke("Value 4", function);

        Assert.Equal(1, functionInvocationCount);

        timeProvider.Advance(intervallTime / 2);

        Assert.Equal(1, functionInvocationCount);

        timeProvider.Advance(intervallTime / 2);

        Assert.Equal(2, functionInvocationCount);
        Assert.Equal("Value 4", lastValue);
    }

    [Fact]
    public void Throttle_Throwing()
    {
        var timeProvider = new FakeTimeProvider();
        var intervallTime = TimeSpan.FromMilliseconds(100);

        var throttle = new Throttle(intervallTime, timeProvider);

        int functionInvocationCount = 0;

        var function = async () =>
        {
            functionInvocationCount++;
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        throttle.Invoke(async () =>
        {
            await function();
            throw new TestException();
        });

        Assert.Equal(1, functionInvocationCount);

        throttle.Invoke(function);
        throttle.Invoke(function);
        throttle.Invoke(function);

        Assert.Equal(1, functionInvocationCount);

        timeProvider.Advance(intervallTime);

        Assert.Equal(2, functionInvocationCount);
    }
}
