using System.Diagnostics;

namespace Essentials.NET.Logging;

public class TraceAppender : ILogAppender
{
    public void Append(LogLevel level, string message)
    {
        Trace.Write(message);
    }

    public void Dispose()
    {
        // not needed
    }
}
