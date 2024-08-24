using Essentials.NET.Logging;
using Xunit;

namespace Essentials.NET.Test.Logging;

public class LoggerTest
{
    private class TestAppender : ILogAppender
    {
        public string Text { get; private set; } = string.Empty;

        public void Append(LogLevel level, string message)
        {
            Text += message + "\n";
        }

        public void Dispose()
        {
            // not needed
        }
    }


    [Fact]
    public void CallsAppender()
    {
        var appender = new TestAppender();

        var logger = new Logger([appender]);

        logger.Info("Message 1");
        logger.Info("Message 2");

        Assert.Contains("Message 1", appender.Text);
        Assert.Contains("Message 2", appender.Text);
    }

    [Fact]
    public void CallsAppender_viaStaticLogClass()
    {
        var appender = new TestAppender();

        Log.Configure(new Logger([appender]));

        Log.Info("Message 1");
        Log.Info("Message 2");

        Assert.Contains("Message 1", appender.Text);
        Assert.Contains("Message 2", appender.Text);
    }

}
