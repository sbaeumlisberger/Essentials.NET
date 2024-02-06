using System.Collections.Concurrent;
using Xunit;

namespace Essentials.NET.Test;

public class AsyncCacheTest
{
    private record ValueObject(string Value);

    private const int CacheSize = 3;

    private readonly List<ValueObject> removedCallbackInvocations = new();

    private readonly AsyncCache<string, ValueObject> cache;

    public AsyncCacheTest()
    {
        cache = new AsyncCache<string, ValueObject>(CacheSize, removedCallbackInvocations.Add);
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesValue_WhenCachedTaskIsCanceled()
    {
        string key = "some-key";
        try
        {
            var tsc = new TaskCompletionSource();
            var cancellationToken = new CancellationToken(true);
            var task = cache.GetOrCreateAsync(key, async (_, ct) =>
            {
                await tsc.Task;
                ct.ThrowIfCancellationRequested();
                return new ValueObject("unreachable");
            }, cancellationToken);
            tsc.SetResult();
            await task;
        }
        catch (OperationCanceledException) { }

        Assert.Equal(0, cache.CurrentSize);

        var value = new ValueObject("some-value");
        var result = await cache.GetOrCreateAsync(key, (_, _) => Task.FromResult(value));

        Assert.Equal(value, result);
        Assert.Equal(1, cache.CurrentSize);
    }

    [Fact]
    public void GetOrCreateAsync_IsThreadSafe()
    {
        string key = "some-key";

        var results = new ConcurrentBag<ValueObject>();

        var threads = Enumerable.Range(0, 100).Select(i => new Thread(() =>
        {
            results.Add(cache.GetOrCreateAsync(key, async (_, _) =>
            {
                await Task.Delay(100);
                return new ValueObject("some-value-" + i);
            }, CancellationToken.None).Result);
        })).ToList();

        threads.ForEach(thread => thread.Start());

        threads.ForEach(thread => thread.Join());

        Assert.Equal(threads.Count, results.Count);
        Assert.True(results.AllEqual());
    }

    [Fact]
    public async void RemoveCallbackIsInvoked_WhenRemovedTaskCompletes()
    {
        var value1 = new ValueObject("test-01");
        var tsc1 = new TaskCompletionSource<ValueObject>();
        _ = cache.GetOrCreateAsync("key-01", (_, _) => tsc1.Task);

        _ = cache.GetOrCreateAsync("key-02", (_, _) => new TaskCompletionSource<ValueObject>().Task);
        _ = cache.GetOrCreateAsync("key-03", (_, _) => new TaskCompletionSource<ValueObject>().Task);
        _ = cache.GetOrCreateAsync("key-04", (_, _) => new TaskCompletionSource<ValueObject>().Task);

        tsc1.SetResult(value1);

        await tsc1.Task.ContinueWith(_ => { });

        Assert.Contains(value1, removedCallbackInvocations);
        Assert.Equal(CacheSize, cache.CurrentSize);
    }

}
