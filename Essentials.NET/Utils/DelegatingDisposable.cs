namespace Essentials.NET;

public class DelegatingDisposable : IDisposable
{
    private readonly Action onDispose;

    private bool isDisposed = false;

    public DelegatingDisposable(Action onDispose)
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

