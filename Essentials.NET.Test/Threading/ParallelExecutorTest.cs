using Xunit;

namespace Essentials.NET.Test;

public class ParallelExecutorTest
{

    private readonly List<string> source = Enumerable.Range(0, 100).Select(i => "Value " + i).ToList();

    public ParallelExecutorTest()
    {
        TaskScheduler.UnobservedTaskException += (sender, args) =>
        {
            Assert.Fail("Unobserved task exception: " + args.Exception);
        };
    }

    [Fact]
    public async Task ProcessAsync_CanceledTask()
    {
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await source.Parallel().ProcessAsync(_ =>
            {
                return Task.FromCanceled(new CancellationToken(true));
            });
        });

    }

    [Fact]
    public async Task ProcessAsync_Cancel()
    {
        var cancellationToken = new CancellationToken(true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await source.Parallel(cancellationToken).ProcessAsync(_ =>
            {
                return Task.CompletedTask;
            });
        });
    }

    [Fact]
    public async Task ProcessAsync_MaxDegreeOfParallelism_Default()
    {
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        cts.Cancel();

        int runningsTasksCount = 0;

        var tcs = new TaskCompletionSource();

        _ = source.Parallel().ProcessAsync(_ =>
        {
            runningsTasksCount++;
            return tcs.Task;
        });

        await Task.Delay(10);

        Assert.Equal(Environment.ProcessorCount, runningsTasksCount);
    }


    [Fact]
    public async Task ProcessAsync_MaxDegreeOfParallelism()
    {
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        cts.Cancel();

        int runningsTasksCount = 0;

        var tcs = new TaskCompletionSource();

        _ = source.Parallel(maxParallelTasks: 2).ProcessAsync(_ =>
        {
            runningsTasksCount++;
            return tcs.Task;
        });

        await Task.Delay(10);

        Assert.Equal(2, runningsTasksCount);
    }

    [Fact]
    public async Task ProcessAsync_ResultsHaveSameOrder()
    {
        var results = await source.Parallel().ProcessAsync(value =>
        {
            return Task.FromResult(value.Replace("Value", "Result"));
        });

        for (int i = 0; i < source.Count; i++)
        {
            Assert.Equal("Result " + i, results[i]);
        }
    }

    [Fact]
    public async Task TryProcessAsync_NoFailures()
    {
        var result = await source.Parallel().TryProcessAsync(value =>
        {
            return Task.FromResult(value.Replace("Value", "Result"));
        });

        Assert.True(result.IsSuccessfully);
        Assert.False(result.HasFailures);
        Assert.Equal(100, result.ProcessedElements.Count);
        Assert.Equal(100, result.Values.Count);
        Assert.Empty(result.Failures);

        for (int i = 0; i < source.Count; i++)
        {
            Assert.Equal("Result " + i, result.Values[i]);
        }
    }

    [Fact]
    public async Task TryProcessAsync_WithFailures()
    {
        var result = await source.Parallel().TryProcessAsync(value =>
        {
            if (value.EndsWith("9"))
            {
                throw new Exception();
            }
            return Task.FromResult(value.Replace("Value", "Result"));
        });

        Assert.False(result.IsSuccessfully);
        Assert.True(result.HasFailures);
        Assert.Equal(90, result.ProcessedElements.Count);
        Assert.Equal(90, result.Values.Count);
        Assert.Equal(10, result.Failures.Count);
    }

}
