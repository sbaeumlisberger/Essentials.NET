namespace Essentials.NET.Logging;

[Obsolete("Use TraceAppender")]
public class DebugAppender : ILogAppender
{
    public void Append(LogLevel level, string message)
    {
        DebugHelper.Write(message);
    }

    public void Dispose()
    {
        // not needed
    }
}
