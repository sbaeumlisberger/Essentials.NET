namespace Essentials.NET;

public static class ParallelExecutor
{
    public static async Task<ParallelResult<T>> TryProcessParallelAsync<T>(
        this IReadOnlyList<T> elements,
        Func<T, Task> processElement,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        Action<(T Element, Exception? Exception)>? callback = null,
        int? maxParallelTasks = null)
    {
        return await TryProcessParallelAsync<T, object?>(
            elements,
            async (element, cancellationToken) =>
            {
                await processElement(element).ConfigureAwait(false);
                return null;
            },
            cancellationToken,
            progress,
            callback,
            maxParallelTasks).ConfigureAwait(false);
    }

    public static async Task<ParallelResult<T>> TryProcessParallelAsync<T>(
        this IReadOnlyList<T> elements,
        Func<T, CancellationToken, Task> processElement,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        Action<(T Element, Exception? Exception)>? callback = null,
        int? maxParallelTasks = null)
    {
        return await TryProcessParallelAsync<T, object?>(
            elements,
            async (element, cancellationToken) =>
            {
                await processElement(element, cancellationToken).ConfigureAwait(false);
                return null;
            },
            cancellationToken,
            progress,
            callback,
            maxParallelTasks).ConfigureAwait(false);
    }

    public static async Task<ParallelResult<TSource, TResult>> TryProcessParallelAsync<TSource, TResult>(
        this IReadOnlyList<TSource> elements,
        Func<TSource, Task<TResult>> processElement,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        Action<(TSource Element, Exception? Exception)>? callback = null,
        int? maxParallelTasks = null)
    {
        return await TryProcessParallelAsync(
            elements,
            (element, _) => processElement(element),
            cancellationToken,
            progress,
            callback,
            maxParallelTasks).ConfigureAwait(false);
    }

    public static async Task<ParallelResult<TSource, TResult>> TryProcessParallelAsync<TSource, TResult>(
        this IReadOnlyList<TSource> elements,
        Func<TSource, CancellationToken, Task<TResult>> processElement,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        Action<(TSource Element, Exception? Exception)>? callback = null,
        int? maxParallelTasks = null)
    {
        if (elements.Count == 0)
        {
            return ParallelResult<TSource, TResult>.Empty;
        }

        var results = new object?[elements.Count];
        int processedElementsCount = 0;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxParallelTasks ?? Environment.ProcessorCount,
            CancellationToken = cancellationToken
        };

        var range = Enumerable.Range(0, elements.Count);
        var task = Parallel.ForEachAsync(range, parallelOptions, async (index, cancellationToken) =>
        {
            var element = elements[index];

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var value = await processElement(element, cancellationToken).ConfigureAwait(false);
                results[index] = new Success<TSource, TResult>(element, value);
                callback?.Invoke((element, null));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                results[index] = (element, exception);
                callback?.Invoke((element, exception));
            }

            progress?.Report(Interlocked.Increment(ref processedElementsCount) / (double)elements.Count);
        });

        await task.ConfigureAwait(false);

        var processedElements = results.OfType<Success<TSource, TResult>>().Select(result => result.Element).ToList();
        var values = results.OfType<Success<TSource, TResult>>().Select(result => result.Value).ToList();
        var failures = results.OfType<Failure<TSource>>().Select(failure => (failure.Element, failure.Exception)).ToList();

        return new ParallelResult<TSource, TResult>(processedElements, values, failures);
    }

    public static async Task ProcessParallelAsync<TSource>(
      this IReadOnlyList<TSource> elements,
      Func<TSource, Task> processElement,
      CancellationToken cancellationToken = default,
      IProgress<double>? progress = null,
      Action<TSource>? callback = null,
      int? maxParallelTasks = null)
    {
        await ProcessParallelAsync<TSource, object?>(
            elements,
            async (element, cancellationToken) =>
            {
                await processElement(element).ConfigureAwait(false);
                return null;
            },
            cancellationToken,
            progress,
            callback,
            maxParallelTasks).ConfigureAwait(false);
    }

    public static async Task ProcessParallelAsync<TSource>(
        this IReadOnlyList<TSource> elements,
        Func<TSource, CancellationToken, Task> processElement,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        Action<TSource>? callback = null,
        int? maxParallelTasks = null)
    {
        await ProcessParallelAsync<TSource, object?>(
            elements,
            async (element, cancellationToken) =>
            {
                await processElement(element, cancellationToken).ConfigureAwait(false);
                return null;
            },
            cancellationToken,
            progress,
            callback,
            maxParallelTasks).ConfigureAwait(false);
    }

    public static async Task<TResult[]> ProcessParallelAsync<TSource, TResult>(
        this IReadOnlyList<TSource> elements,
        Func<TSource, Task<TResult>> processElement,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        Action<TSource>? callback = null,
        int? maxParallelTasks = null)
    {
        return await ProcessParallelAsync(
            elements,
            (element, _) => processElement(element),
            cancellationToken,
            progress,
            callback,
            maxParallelTasks).ConfigureAwait(false);

    }
    public static async Task<TResult[]> ProcessParallelAsync<TSource, TResult>(
        this IReadOnlyList<TSource> elements,
        Func<TSource, CancellationToken, Task<TResult>> processElement,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        Action<TSource>? callback = null,
        int? maxParallelTasks = null)
    {
        if (elements.Count == 0)
        {
            return Array.Empty<TResult>();
        }

        var results = new TResult[elements.Count];
        int processedElementsCount = 0;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxParallelTasks ?? Environment.ProcessorCount,
            CancellationToken = cancellationToken
        };

        var range = Enumerable.Range(0, elements.Count);
        var task = Parallel.ForEachAsync(range, parallelOptions, async (index, cancellationToken) =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            var element = elements[index];
            var value = await processElement(element, cancellationToken).ConfigureAwait(false);
            results[index] = value;
            callback?.Invoke(element);
            progress?.Report(Interlocked.Increment(ref processedElementsCount) / (double)elements.Count);
        });

        await task.ConfigureAwait(false);

        return results;
    }

    private record Failure<T>(T Element, Exception Exception);

    private record Success<T, TOut>(T Element, TOut Value);
}

public class ParallelResult<T>
{
    public static ParallelResult<T> Empty { get; } = new();

    public bool IsSuccessfully => !HasFailures;

    public bool HasFailures => Failures.Any();

    public IReadOnlyList<T> ProcessedElements { get; }

    public IReadOnlyList<(T Element, Exception Exception)> Failures { get; }

    protected ParallelResult()
    {
        ProcessedElements = Array.Empty<T>();
        Failures = Array.Empty<(T, Exception)>();
    }

    public ParallelResult(IReadOnlyList<T> processedElements, IReadOnlyList<(T, Exception)> failures)
    {
        ProcessedElements = processedElements;
        Failures = failures;
    }
}

public class ParallelResult<TSource, TResult> : ParallelResult<TSource>
{
    public static new ParallelResult<TSource, TResult> Empty { get; } = new();

    public IReadOnlyList<TResult> Values { get; }

    private ParallelResult()
    {
        Values = Array.Empty<TResult>();
    }

    public ParallelResult(
        IReadOnlyList<TSource> processedElements,
        IReadOnlyList<TResult> values,
        IReadOnlyList<(TSource, Exception)> failures)
        : base(processedElements, failures)
    {
        Values = values;
    }
}