using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Essentials.NET.Test.Threading;

public class DebouncerTest : IDisposable
{
    private readonly FakeTimeProvider timeProvider = new FakeTimeProvider();

    private readonly FakeSynchronizationContext fakeSynchronizationContext = new FakeSynchronizationContext();

    private readonly SynchronizationContext? originalSynchronizationContext;

    public DebouncerTest()
    {
        originalSynchronizationContext = SynchronizationContext.Current;
        SynchronizationContext.SetSynchronizationContext(fakeSynchronizationContext);
    }

    public void Dispose()
    {
        SynchronizationContext.SetSynchronizationContext(originalSynchronizationContext);
    }

    [Fact]
    public void MultipleInvocations()
    {
        var start = timeProvider.GetTimestamp();

        var functionInvocations = new List<double>();

        var function = async () =>
        {
            functionInvocations.Add(timeProvider.GetElapsedTime(start).TotalMilliseconds);
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var debounce = new Debouncer(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        debounce.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(100));
        debounce.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(100));
        debounce.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(100));
        debounce.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(2000));

        Assert.Equal([800], functionInvocations);
    }

    [Fact]
    public void OneInvocation()
    {
        var start = timeProvider.GetTimestamp();

        var functionInvocations = new List<double>();

        var function = async () =>
        {
            functionInvocations.Add(timeProvider.GetElapsedTime(start).TotalMilliseconds);
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var debounce = new Debouncer(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        debounce.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(2000));

        Assert.Equal([500], functionInvocations);
    }

    [Fact]
    public async Task CapturesSynchronizationContext()
    {
        int functionInvocationCount = 0;
        SynchronizationContext? functionSynchronizationContext = null;

        var function = async () =>
        {
            functionInvocationCount++;
            functionSynchronizationContext = SynchronizationContext.Current;
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var debounce = new Debouncer(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        debounce.Invoke();

        await Task.Run(() =>
        {
            timeProvider.Advance(TimeSpan.FromMilliseconds(500));
        });

        Assert.Equal(1, functionInvocationCount);
        Assert.Equal(fakeSynchronizationContext, functionSynchronizationContext);
    }

    [Fact]
    public void IgnoresAsyncExceptions()
    {
        var intervalTime = TimeSpan.FromMilliseconds(500);
        int functionInvocationCount = 0;
        var function = async () =>
        {
            functionInvocationCount++;
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
            throw new Exception("Some exception");
        };

        var debounce = new Debouncer(intervalTime, function, true, timeProvider);

        debounce.Invoke();

        Assert.Equal(0, functionInvocationCount);

        AdvanceTimeStepByStep(intervalTime);

        Assert.Equal(1, functionInvocationCount);

        debounce.Invoke();

        AdvanceTimeStepByStep(intervalTime);

        Assert.Equal(2, functionInvocationCount);
    }

    [Fact]
    public void IgnoresSyncExceptions()
    {
        var intervalTime = TimeSpan.FromMilliseconds(500);
        int functionInvocationCount = 0;
        var function = () =>
        {
            functionInvocationCount++;
            bool throws = true;
            if (throws)
            {
                throw new Exception("Some exception");
            }
        };

        var debounce = new Debouncer(intervalTime, function, true, timeProvider);

        debounce.Invoke();

        Assert.Equal(0, functionInvocationCount);

        timeProvider.Advance(intervalTime);

        Assert.Equal(1, functionInvocationCount);

        debounce.Invoke();

        timeProvider.Advance(intervalTime);

        Assert.Equal(2, functionInvocationCount);
    }

    [Fact]
    public void FunctionTakesLongerThanTheInterval()
    {
        //      0          550        
        //  IN  X          X          
        //      <--------> <-------->   
        //  OUT           X          X           
        //                500        1050
        // EXEC           <-------------->
        //                           <-------------->

        var start = timeProvider.GetTimestamp();

        var functionInvocations = new List<double>();

        var function = async () =>
        {
            functionInvocations.Add(timeProvider.GetElapsedTime(start).TotalMilliseconds);
            await Task.Delay(TimeSpan.FromMilliseconds(800), timeProvider);
        };

        var debounce = new Debouncer(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        debounce.Invoke();
        timeProvider.Advance(TimeSpan.FromMilliseconds(500));
        timeProvider.Advance(TimeSpan.FromMilliseconds(50));
        debounce.Invoke();
        timeProvider.Advance(TimeSpan.FromMilliseconds(500));
        timeProvider.Advance(TimeSpan.FromMilliseconds(250));

        Assert.Equal([500, 1050], functionInvocations);
    }

    [Fact]
    public void FunctionTakesLongerThanTheInterval2()
    {
        //      0          550        1100
        //  IN  X          X          X
        //      <--------> <--------> <-------->   
        //  OUT           X          X          X         
        //                500        1050       1600
        // EXEC           <-------------->      
        //                           <-------------->
        //                                      <-------------->

        var start = timeProvider.GetTimestamp();

        var functionInvocations = new List<double>();

        var function = async () =>
        {
            functionInvocations.Add(timeProvider.GetElapsedTime(start).TotalMilliseconds);
            await Task.Delay(TimeSpan.FromMilliseconds(800), timeProvider);
        };

        var debounce = new Debouncer(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        debounce.Invoke();
        timeProvider.Advance(TimeSpan.FromMilliseconds(500));
        timeProvider.Advance(TimeSpan.FromMilliseconds(50));
        debounce.Invoke();
        timeProvider.Advance(TimeSpan.FromMilliseconds(500));
        timeProvider.Advance(TimeSpan.FromMilliseconds(50));
        debounce.Invoke();
        timeProvider.Advance(TimeSpan.FromMilliseconds(200));
        timeProvider.Advance(TimeSpan.FromMilliseconds(300));

        Assert.Equal([500, 1050, 1600], functionInvocations);
    }


    [Fact]
    public void Flush_Immediately()
    {
        var start = timeProvider.GetTimestamp();

        var functionInvocations = new List<double>();

        var function = async () =>
        {
            functionInvocations.Add(timeProvider.GetElapsedTime(start).TotalMilliseconds);
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var debounce = new Debouncer(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        debounce.Invoke();
        debounce.Invoke();
        debounce.Invoke();

        debounce.Flush();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(2000));

        Assert.Equal([0], functionInvocations);
    }

    [Fact]
    public void Flush_WithDelay()
    {
        var start = timeProvider.GetTimestamp();

        var functionInvocations = new List<double>();

        var function = async () =>
        {
            functionInvocations.Add(timeProvider.GetElapsedTime(start).TotalMilliseconds);
            await Task.Delay(TimeSpan.FromMilliseconds(10), timeProvider);
        };

        var debounce = new Debouncer(TimeSpan.FromMilliseconds(500), function, true, timeProvider);

        debounce.Invoke();
        debounce.Invoke();
        debounce.Invoke();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(100));
        debounce.Flush();

        AdvanceTimeStepByStep(TimeSpan.FromMilliseconds(2000));

        Assert.Equal([100], functionInvocations);
    }

    // TODO: test with input

    // TODO: dispose

    private void AdvanceTimeStepByStep(TimeSpan timeSpan)
    {
        for (int i = 0; i < timeSpan.TotalMilliseconds; i++)
        {
            timeProvider.Advance(TimeSpan.FromMilliseconds(1));
        }
    }
}
