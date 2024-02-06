using Xunit;

namespace Essentials.NET.Test;

public class TimeSpanFormatterTest
{

    [Fact]
    public void ToReadableString()
    {
        Assert.Equal("< 1s", TimeSpan.FromMilliseconds(500).ToReadableString());
        Assert.Equal("1s", TimeSpan.FromMilliseconds(1500).ToReadableString());
        Assert.Equal("15s", TimeSpan.FromSeconds(15).ToReadableString());
        Assert.Equal("7min 30s", TimeSpan.FromSeconds(450).ToReadableString());
        Assert.Equal("1s 500ms", TimeSpan.FromMilliseconds(1500).ToReadableString(TimeUnit.Millisecond));
        Assert.Equal("< 1d", TimeSpan.FromHours(1).ToReadableString(TimeUnit.Day));
    }

}
