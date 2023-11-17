namespace Essentials.NET;

public class Disposable : IDisposable
{
    private readonly Action onDispose;

    private bool isDisposed = false;

    public Disposable(Action onDispose)
    {
        this.onDispose = onDispose;
    }

    public void Dispose()
    {
        if (!isDisposed)
        {
            onDispose();
            isDisposed = true;
        }
    }
}

