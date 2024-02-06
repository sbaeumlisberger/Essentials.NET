using Xunit;

namespace Essentials.NET.Test;

public class AsyncLockTest
{

    [Fact]
    public async Task AcquireAsync_CanOnlyBeAcquiredByOneThreadSimultaneously()
    {
        var asyncLock = new AsyncLock();

        Task<IDisposable> acquireTask1 = null!;
        Task<IDisposable> acquireTask2 = null!;
        Task<IDisposable> acquireTask3 = null!;

        await Task.Run(() => { acquireTask1 = asyncLock.AcquireAsync(); });
        await Task.Run(() => { acquireTask2 = asyncLock.AcquireAsync(); });
        await Task.Run(() => { acquireTask3 = asyncLock.AcquireAsync(); });

        var lock1 = await acquireTask1;

        Assert.True(acquireTask1.IsCompleted);
        Assert.False(acquireTask2.IsCompleted);
        Assert.False(acquireTask3.IsCompleted);

        lock1.Dispose();

        var lock2 = await acquireTask2.WaitAsync(TimeSpan.FromMilliseconds(10));

        Assert.True(acquireTask1.IsCompleted);
        Assert.True(acquireTask2.IsCompleted);
        Assert.False(acquireTask3.IsCompleted);

        lock2.Dispose();

        await acquireTask3.WaitAsync(TimeSpan.FromMilliseconds(10));

        Assert.True(acquireTask1.IsCompleted);
        Assert.True(acquireTask2.IsCompleted);
        Assert.True(acquireTask3.IsCompleted);
    }

    [Fact]
    public void AcquireAsync_ReturnsCompletedTaskIfNotAcquiredByOtherThread()
    {
        var asyncLock = new AsyncLock();

        var acquireTask = asyncLock.AcquireAsync();

        Assert.True(acquireTask.IsCompleted);
    }
}
