namespace Essentials.NET;

public static class ParallelExecutor
{
    /// <summary>
    /// Creates a parallel executor that can be used to process the elements in parallel.
    /// </summary>
    public static ParallelExecutor<T> Parallel<T>(this IReadOnlyList<T> elements, CancellationToken cancellationToken = default, IProgress<double>? progress = null)
    {
        return new ParallelExecutor<T>(elements, cancellationToken, progress);
    }
}

public class ParallelExecutor<T>(
    IReadOnlyList<T> elements,
    CancellationToken cancellationToken = default, 
    IProgress<double>? progress = null)
{
    private Action<(T Element, Exception? Exception)>? callback = null;

    private int maxDegreeOfParallelism = Environment.ProcessorCount;     

    public ParallelExecutor<T> WithCallback(Action<(T Element, Exception? Exception)> callback)
    {
        this.callback = callback;
        return this;
    }

    public ParallelExecutor<T> WithMaxDegreeOfParallelism(int maxDegreeOfParallelism)
    {
        this.maxDegreeOfParallelism = maxDegreeOfParallelism;
        return this;
    }

    /// <summary>Processes each element in parallel using the specified function.</summary>
    /// <exception cref="OperationCanceledException"/>
    /// <exception cref="AggregateException"/>
    public async Task ProcessAsync(Func<T, Task> processElement)
    {
        await ProcessAsync((element, _) => processElement(element)).ConfigureAwait(false);
    }

    /// <summary>Processes each element in parallel using the specified function.</summary>
    /// <exception cref="OperationCanceledException"/>
    /// <exception cref="AggregateException"/>
    public async Task ProcessAsync(Func<T, CancellationToken, Task> processElement)
    {
        await ProcessAsync<object?>(async (element, cancellationToken) => { await processElement(element, cancellationToken); return null; }).ConfigureAwait(false);
    }

    /// <summary>Processes each element in parallel using the specified function.</summary>
    /// <returns>An array with the result of each element in the same order as the input.</returns>
    /// <exception cref="OperationCanceledException"/>
    /// <exception cref="AggregateException"/>
    public async Task<TOut[]> ProcessAsync<TOut>(Func<T, Task<TOut>> processElement)
    {
        return await ProcessAsync((element, _) => processElement(element)).ConfigureAwait(false);
    }

    /// <summary>Processes each element in parallel using the specified function.</summary>
    /// <returns>An array with the result of each element in the same order as the input.</returns>
    /// <exception cref="OperationCanceledException"/>
    /// <exception cref="AggregateException"/>
    public async Task<TOut[]> ProcessAsync<TOut>(Func<T, CancellationToken, Task<TOut>> processElement)
    {
        if (elements.Count == 0)
        {
            return Array.Empty<TOut>();
        }

        var results = new TOut[elements.Count];
        int processedElementsCount = 0;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
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
                results[index] = value;
                callback?.Invoke((element, null));
                progress?.Report(Interlocked.Increment(ref processedElementsCount) / (double)elements.Count);
            }
            catch (Exception exception)
            {
                callback?.Invoke((element, exception));
                throw;
            }
        });

        await task.ConfigureAwait(false);

        return results;
    }

    /// <summary>Tries to process each element in parallel using the specified function.</summary>
    /// <returns>An object that contains lists with the successfully processed elements and the failures.</returns>
    /// <exception cref="OperationCanceledException"/>
    public async Task<ParallelResult<T>> TryProcessAsync(Func<T, Task> processElement)
    {
        return await TryProcessAsync((element, _) => processElement(element)).ConfigureAwait(false);
    }

    /// <summary>Tries to process each element in parallel using the specified function.</summary>
    /// <returns>An object that contains lists with the successfully processed elements and the failures.</returns>
    /// <exception cref="OperationCanceledException"/>
    public async Task<ParallelResult<T>> TryProcessAsync(Func<T, CancellationToken, Task> processElement)
    {
        return await TryProcessAsync<object?>(async (element, cancellationToken) => { await processElement(element, cancellationToken); return null; }).ConfigureAwait(false);
    }

    /// <summary>Tries to process each element in parallel using the specified function.</summary>
    /// <returns>An object that contains lists with the successfully processed elements, the result values and the failures.</returns>
    /// <exception cref="OperationCanceledException"/>
    public async Task<ParallelResult<T, TOut>> TryProcessAsync<TOut>(Func<T, Task<TOut>> processElement)
    {
        return await TryProcessAsync((element, _) => processElement(element)).ConfigureAwait(false);
    }

    /// <summary>Tries to process each element in parallel using the specified function.</summary>
    /// <returns>An object that contains lists with the successfully processed elements, the result values and the failures.</returns>
    /// <exception cref="OperationCanceledException"/>
    public async Task<ParallelResult<T, TOut>> TryProcessAsync<TOut>(Func<T, CancellationToken, Task<TOut>> processElement)
    {
        if (elements.Count == 0)
        {
            return ParallelResult<T, TOut>.Empty;
        }

        var results = new object?[elements.Count];
        int processedElementsCount = 0;

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = maxDegreeOfParallelism,
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
                results[index] = new Success<TOut>(element, value);
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

        var processedElements = results.OfType<Success<TOut>>().Select(result => result.Element).ToList();
        var values = results.OfType<Success<TOut>>().Select(result => result.Value).ToList();
        var failures = results.OfType<Failure>().Select(failure => (failure.Element, failure.Exception)).ToList();

        return new ParallelResult<T, TOut>(processedElements, values, failures);
    }

    private record Failure(T Element, Exception Exception);

    private record Success<TOut>(T Element, TOut Value);
}