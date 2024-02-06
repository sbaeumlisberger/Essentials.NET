using Xunit;

namespace Essentials.NET.Test;

public class ShareableDisposableTest
{
    private class ShareableDisposableImpl : ShareableDisposable
    {
        public bool IsDisposed { get; private set; } = false;

        protected override void OnDispose()
        {
            IsDisposed = true;
        }
    }

    [Fact]
    public void OnDisposeIsCalled_WhenCreatedAndDisposed()
    {
        var sharedDisposable = new ShareableDisposableImpl();
        sharedDisposable.Dispose();
        Assert.True(sharedDisposable.IsDisposed);
    }

    [Fact]
    public void OnDisposeIsCalled_WhenDisposedSameTimesAsRequested()
    {
        var sharedDisposable = new ShareableDisposableImpl();
        sharedDisposable.RequestUsage();

        sharedDisposable.Dispose();
        sharedDisposable.Dispose();

        Assert.True(sharedDisposable.IsDisposed);
    }

    [Fact]
    public void OnDisposeIsNotCalled_WhenDisposedLessTimesThanRequested()
    {
        var sharedDisposable = new ShareableDisposableImpl();
        sharedDisposable.RequestUsage();

        sharedDisposable.Dispose();

        Assert.False(sharedDisposable.IsDisposed);
    }

}
