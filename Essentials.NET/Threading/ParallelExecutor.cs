namespace Essentials.NET;

public static class ParallelExecutor
{
    /// <summary>
    /// Creates a parallel executor that can be used to process the elements in parallel.
    /// </summary>
    /// <param name="elements">The elements to process in parallel.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the processing.</param>
    /// <param name="progress">A progress object to report the progress of the processing.</param>
    /// <param name="callback">A callback that is called when an element was processed. The callback receives the element and the exception if any.</param>
    /// <param name="errorCallback">A callback that is called when an processing of an element has failed. The callback receives the element and the exception.</param>
    /// <param name="maxParallelTasks">The maximum number of parallel tasks. If not specified, <see cref="Environment.ProcessorCount"/> is used.</param>
    public static ParallelExecutor<T> Parallel<T>(
        this IReadOnlyList<T> elements,
        CancellationToken cancellationToken = default,
        IProgress<double>? progress = null,
        Action<(T Element, Exception? Exception)>? callback = null,
        Action<(T Element, Exception Exception)>? failureCallback = null,
        int? maxParallelTasks = null)
    {
        return new ParallelExecutor<T>(elements, cancellationToken, progress, callback, failureCallback, maxParallelTasks);
    }
}

public class ParallelExecutor<T>(
    IReadOnlyList<T> elements,
    CancellationToken cancellationToken = default,
    IProgress<double>? progress = null,
    Action<(T Element, Exception? Exception)>? callback = null,
    Action<(T Element, Exception Exception)>? failureCallback = null,
    int? maxParallelTasks = null)
{
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
                results[index] = new Success<TOut>(element, value);
                callback?.Invoke((element, null));
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                results[index] = new Failure(element, exception);
                callback?.Invoke((element, exception));
                failureCallback?.Invoke((element, exception));
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