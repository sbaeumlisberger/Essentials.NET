using Xunit;

namespace Essentials.NET.Test.Threading;

public class CancelableTaskRunnerTest
{
    [Fact]
    public async void RunAndCancelPrevious()
    {
        var cancelableTaskRunner = new CancelableTaskRunner();

        var executed = new List<string>();

        var longRunningTaskSource = new TaskCompletionSource();
        var longRunningTask = longRunningTaskSource.Task;

        var task1 = cancelableTaskRunner.RunAndCancelPrevious(async (cancellationToken) =>
        {
            executed.Add("01");
            await longRunningTask;
            cancellationToken.ThrowIfCancellationRequested();
            executed.Add("02");
        });

        var task2 = cancelableTaskRunner.RunAndCancelPrevious(async (cancellationToken) =>
        {
            executed.Add("03");
            await Task.Yield();
        });

        longRunningTaskSource.SetResult();

        await task1;
        await task2;

        Assert.Equal(new[] { "01", "03" }, executed);
    }

    [Fact(Timeout = 1000)]
    public async void RunAndCancelPrevious_MultiThread()
    {
        var cancelableTaskRunner = new CancelableTaskRunner();

        var executed = new List<string>();

        var thread2Lock = new SemaphoreSlim(0, 1);

        var thread1 = Task.Run(() =>
        {
            ;
            return cancelableTaskRunner.RunAndCancelPrevious(async (cancellationToken) =>
            {
                executed.Add("01");
                await Task.Yield();
                cancellationToken.ThrowIfCancellationRequested();
                thread2Lock.Release();
                executed.Add("02");
            });
        });

        var thread2 = Task.Run(() =>
        {
            thread2Lock.Wait();
            return cancelableTaskRunner.RunAndCancelPrevious(async (cancellationToken) =>
            {
                executed.Add("03");
                await Task.Yield();
            });
        });

        await thread1;
        await thread2;

        Assert.Equal(new[] { "01", "02", "03" }, executed);
    }


    [Fact]
    public async void RunAndCancelPrevious_Throwing()
    {
        var cancelableTaskRunner = new CancelableTaskRunner();

        bool task2Executed = false;

        var task1 = cancelableTaskRunner.RunAndCancelPrevious(async (cancellationToken) =>
        {
            await Task.Yield();
            throw new TestException();
        });

        var task2 = cancelableTaskRunner.RunAndCancelPrevious(async (cancellationToken) =>
        {
            await Task.Yield();
            task2Executed = true;
        });

        await Assert.ThrowsAsync<TestException>(async () => await task1);
        await task2;
        Assert.True(task2Executed);
    }
}
