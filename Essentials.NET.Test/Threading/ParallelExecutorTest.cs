using System.Linq;
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
    public async Task ProcessParallelAsync_CanceledTask()
    {
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await source.Parallel().ProcessAsync(_ =>
            {
                cts.Cancel();
                return Task.FromCanceled(cancellationToken);
            });
        });

    }

    [Fact]
    public async Task ProcessAsync_Cancel()
    {
        var cts = new CancellationTokenSource();
        var cancellationToken = cts.Token;
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
        {
            await source.Parallel(cancellationToken).ProcessAsync(_ =>
            {
                return Task.CompletedTask;
            });
        });
    }

}
