using Microsoft.Extensions.Time.Testing;
using Xunit;
using Xunit.Sdk;

namespace Essentials.NET.Test.Threading;

public class ThrottleTest : IDisposable
{
    private readonly FakeSynchronizationContext fakeSynchronizationContext = new FakeSynchronizationContext();

    private readonly SynchronizationContext? originalSynchronizationContext;

    private readonly FakeTimeProvider timeProvider = new FakeTimeProvider();

    public ThrottleTest()
    {
        originalSynchronizationContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(fakeSynchronizationContext);
    }

    public void Dispose()
    {
        SynchronizationContext.SetSynchronizationContext(originalSynchronizationContext);
    }

    [Fact]
    public async Task CapturesSynchronizationContext()
    {
        SynchronizationContext? functionSynchronizationContext = null;

        var function = async () =>
        {
            functionSynchronizationContext = SynchronizationContext.Current;
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var throttle = new Throttle(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        throttle.Invoke();

        await Task.Run(() => timeProvider.Advance(TimeSpan.FromMilliseconds(500)));

        Assert.Equal(fakeSynchronizationContext, functionSynchronizationContext);
    }

    [Fact]
    public void MultipleInvocationsNew()
    {
        var start = timeProvider.GetTimestamp();

        var functionInvocations = new List<double>();

        var function = async () =>
        {
            functionInvocations.Add(timeProvider.GetElapsedTime(start).TotalMilliseconds);
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var throttle = new Throttle(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        throttle.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(50));
        throttle.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(50));
        throttle.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(50));
        throttle.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(2000));
        throttle.Invoke();

        Assert.Equal([0, 510, 2150], functionInvocations);
    }

    [Fact]
    public void InvocationWhileExecuting()
    {
        var start = timeProvider.GetTimestamp();

        var functionInvocations = new List<double>();

        var executionDuration = TimeSpan.FromMilliseconds(10);

        var function = async () =>
        {
            functionInvocations.Add(timeProvider.GetElapsedTime(start).TotalMilliseconds);
            await Task.Delay(executionDuration, timeProvider);
        };

        var throttle = new Throttle(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        throttle.Invoke();

        AdvanceTimeStepByStep(executionDuration / 2);
        throttle.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(2000));

        Assert.Equal([0, 510], functionInvocations);
    }

    [Fact]
    public void OneInvocation()
    {
        var intervallTime = TimeSpan.FromMilliseconds(100);

        int functionInvocationCount = 0;

        var function = async () =>
        {
            functionInvocationCount++;
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var throttle = new Throttle(intervallTime, function, true, timeProvider);

        throttle.Invoke();

        timeProvider.Advance(intervallTime);

        Assert.Equal(1, functionInvocationCount);

        timeProvider.Advance(intervallTime);
        timeProvider.Advance(intervallTime);

        Assert.Equal(1, functionInvocationCount);
    }

    [Fact]
    public void WithInput()
    {
        var intervallTime = TimeSpan.FromMilliseconds(100);
        var executionTime = TimeSpan.FromMilliseconds(10);

        int functionInvocationCount = 0;
        string? lastValue = null;    

        var function = async (string value) =>
        {
            functionInvocationCount++;
            lastValue = value;
            await Task.Delay(executionTime, timeProvider);
        };

        var throttle = new Throttle<string>(intervallTime, function, true, timeProvider);

        throttle.Invoke("Value 1");

        Assert.Equal(1, functionInvocationCount);
        Assert.Equal("Value 1", lastValue);

        throttle.Invoke("Value 2");
        throttle.Invoke("Value 3");
        throttle.Invoke("Value 4");

        Assert.Equal(1, functionInvocationCount);

        timeProvider.Advance(executionTime);
        timeProvider.Advance(intervallTime / 2);

        Assert.Equal(1, functionInvocationCount);

        timeProvider.Advance(intervallTime / 2);

        Assert.Equal(2, functionInvocationCount);
        Assert.Equal("Value 4", lastValue);
    }

    [Fact]
    public void SyncThrowing()
    {
        var intervallTime = TimeSpan.FromMilliseconds(100);

        int functionInvocationCount = 0;

        bool throws = true;
        var function = async () =>
        {
            functionInvocationCount++;
            if (throws)
            {
                throw new TestException();
            }
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var throttle = new Throttle(intervallTime, function, true, timeProvider);

        throttle.Invoke();

        Assert.Equal(1, functionInvocationCount);

        throws = false;

        throttle.Invoke();
        throttle.Invoke();
        throttle.Invoke();

        Assert.Equal(1, functionInvocationCount);

        timeProvider.Advance(intervallTime);

        Assert.Equal(2, functionInvocationCount);
    }

    [Fact]
    public void AsyncThrowing()
    {
        var intervallTime = TimeSpan.FromMilliseconds(100);
        var executionTime = TimeSpan.FromMilliseconds(10);

        int functionInvocationCount = 0;

        bool throws = true;
        var function = async () =>
        {
            functionInvocationCount++;
            await Task.Delay(executionTime, timeProvider);
            if (throws)
            {
                throw new TestException();
            }
        };

        var throttle = new Throttle(intervallTime, function, true, timeProvider);

        throttle.Invoke();

        Assert.Equal(1, functionInvocationCount);

        throttle.Invoke();
        throttle.Invoke();
        throttle.Invoke();

        Assert.Equal(1, functionInvocationCount);

        timeProvider.Advance(executionTime);

        Assert.Equal(1, functionInvocationCount);

        timeProvider.Advance(intervallTime);

        Assert.Equal(2, functionInvocationCount);
    }

    [Fact]
    public void Flush()
    {
        var start = timeProvider.GetTimestamp();

        var functionInvocations = new List<double>();

        var function = async () =>
        {
            functionInvocations.Add(timeProvider.GetElapsedTime(start).TotalMilliseconds);
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var intervallTime = TimeSpan.FromMilliseconds(100);
        var throttle = new Throttle(intervallTime, function, true, timeProvider);

        throttle.Invoke();
        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(20));
        throttle.Invoke();
        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(20));

        var flushTask = throttle.Flush();
        Assert.False(flushTask.IsCompleted);
        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(10));
        Assert.True(flushTask.IsCompleted);

        Assert.Equal([0, 40], functionInvocations);
    }

    private void AdvanceTimeStepByStep(TimeSpan timeSpan)
    {
        for (int i = 0; i < timeSpan.TotalMilliseconds; i++)
        {
            timeProvider.Advance(TimeSpan.FromMilliseconds(1));
        }
    }
}
