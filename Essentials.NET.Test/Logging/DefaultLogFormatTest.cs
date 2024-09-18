using Essentials.NET.Logging;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Essentials.NET.Test.Logging;

public class DefaultLogFormatTest
{

    [Fact]
    public void Format()
    {
        var timeProvider = new FakeTimeProvider(new DateTimeOffset(2024, 08, 13, 15, 07, 22, 055, TimeSpan.Zero));
        var defaultLogFormat = new DefaultLogFormat(filePathOffset: "prefix/".Length)
        {
            TimeProvider = timeProvider,
            ThreadIdProvider = () => 7
        };

        var result = defaultLogFormat.Format(LogLevel.INFO, "message", null, "memberName", "prefix/filePath", 35);

        Assert.Equal("2024-08-13 15:07:22.055 | INFO  |   7 | filePath:35 | message \n", result);
    }

}
