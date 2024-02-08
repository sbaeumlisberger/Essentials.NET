namespace Essentials.NET;

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
