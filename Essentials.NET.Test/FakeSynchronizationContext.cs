namespace Essentials.NET.Test;

internal class FakeSynchronizationContext : SynchronizationContext
{

    public override void Post(SendOrPostCallback callback, object? state)
    {
        var previousSynchronizationContext = Current;
        SetSynchronizationContext(this);
        try
        {
            callback(state);
        }
        finally
        {
            SetSynchronizationContext(previousSynchronizationContext);
        }
    }

    public override void Send(SendOrPostCallback callback, object? state)
    {
        var previousSynchronizationContext = Current;
        SetSynchronizationContext(this);
        try
        {
            callback(state);
        }
        finally
        {
            SetSynchronizationContext(previousSynchronizationContext);
        }
    }
}
