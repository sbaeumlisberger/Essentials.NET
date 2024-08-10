#define DEBUG // preverse the Debug.Write call on release build

namespace Essentials.NET.Logging;

public class DebugAppender : ILogAppender
{
    public void Append(LogLevel level, string message)
    {
        System.Diagnostics.Debug.Write(message);
    }

    public void Dispose()
    {
        // not needed
    }
}
