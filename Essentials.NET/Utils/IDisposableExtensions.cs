namespace Essentials.NET;

public static class IDisposableExtensions
{
    public static void DisposeSafely<T>(this T? disposable, Action setNull) where T : IDisposable
    {
        var tmp = disposable;
        setNull();
        tmp?.Dispose();
    }
}
