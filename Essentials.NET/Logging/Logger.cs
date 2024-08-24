using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace Essentials.NET.Logging;

public class Logger : ILogger
{
    public IReadOnlyList<ILogAppender> Appenders { get; }

    private readonly ILogFormat logFormat;

    private bool disposed = false;

    public Logger(IEnumerable<ILogAppender>? appenders = null, ILogFormat? logFormat = null)
    {
        Appenders = new ReadOnlyCollection<ILogAppender>(appenders?.ToList() ?? [new DebugAppender()]);
        this.logFormat = logFormat ?? new DefaultLogFormat();
    }

    public void Dispose()
    {
        if (!disposed)
        {
            Appenders.ForEach(appender => appender.Dispose());
            disposed = true;
        }
    }

    public void Debug(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Write(LogLevel.DEBUG, message, exception, filePath, memberName, lineNumber);
    }

    public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Write(LogLevel.INFO, message, null, filePath, memberName, lineNumber);
    }

    public void Warn(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Write(LogLevel.WARN, message, exception, filePath, memberName, lineNumber);
    }

    public void Error(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Write(LogLevel.ERROR, message, exception, filePath, memberName, lineNumber);
    }

    public void Fatal(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Write(LogLevel.FATAL, message, exception, filePath, memberName, lineNumber);
    }

    public void Write(LogLevel level, string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        if (disposed)
        {
            DebugHelper.Write("Can not write log message because the logger has been disposed");
            return;
        }

        Append(level, logFormat.Format(level, message, exception, memberName, filePath, lineNumber));
    }

    private void Append(LogLevel level, string message)
    {
        Appenders.ForEach(appender => appender.Append(level, message));
    }
}
