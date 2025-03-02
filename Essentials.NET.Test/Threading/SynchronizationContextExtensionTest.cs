using Xunit;

namespace Essentials.NET.Test.Threading;

public class SynchronizationContextExtensionTest
{
    private class TestSyncContext : SynchronizationContext
    {
        public override void Send(SendOrPostCallback callback, object? state)
        {
            var thread = new Thread(() =>
            {
                SetSynchronizationContext(this);
                callback(state);
            });
            thread.Start();
            thread.Join();
        }

        public override void Post(SendOrPostCallback callback, object? state)
        {
            new Thread(() =>
            {
                SetSynchronizationContext(this);
                callback(state);
            }).Start();
        }
    }

    private readonly TestSyncContext syncContext = new TestSyncContext();

    [Fact]
    public void Dispatch()
    {
        SynchronizationContext? calledOnSyncContext = null;
        bool completed = false;

        syncContext.Dispatch(() =>
        {
            calledOnSyncContext = SynchronizationContext.Current;
            completed = true;
        });

        Assert.IsType<TestSyncContext>(calledOnSyncContext);
        Assert.True(completed);
    }

    [Fact]
    public void Dispatch_ThrowingAction()
    {
        void ThrowTestException()
        {
            throw new TestException();
        }

        var exception = Assert.Throws<TestException>(() =>
        {
            syncContext.Dispatch(() =>
            {
                ThrowTestException();
            });
        });
        Assert.Contains("ThrowTestException", exception.StackTrace!);
    }

    [Fact]
    public async Task DispatchAsync()
    {
        SynchronizationContext? calledOnSyncContext = null;
        bool completed = false;

        await syncContext.DispatchAsync(() =>
        {
            calledOnSyncContext = SynchronizationContext.Current;
            completed = true;
        });

        Assert.IsType<TestSyncContext>(calledOnSyncContext);
        Assert.True(completed);
    }

    [Fact]
    public async Task DispatchAsync_ThrowingFunction()
    {
        await Assert.ThrowsAsync<TestException>(async () =>
        {
            await syncContext.DispatchAsync(() =>
            {
                throw new TestException();
            });
        });
    }

    [Fact]
    public async Task DispatchAsync_AsyncFuntion()
    {
        SynchronizationContext? calledOnSyncContext = null;
        bool completed = false;

        await syncContext.DispatchAsync(async () =>
        {
            await Task.Delay(1);
            calledOnSyncContext = SynchronizationContext.Current;
            completed = true;
        });

        Assert.IsType<TestSyncContext>(calledOnSyncContext);
        Assert.True(completed);
    }

    [Fact]
    public async Task DispatchAsync_AsyncThrowingFunction()
    {
        await Assert.ThrowsAsync<TestException>(async () =>
        {
            await syncContext.DispatchAsync(async () =>
            {
                await Task.Delay(1);
                throw new TestException();
            });
        });
    }
}
