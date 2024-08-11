using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Essentials.NET.Logging;

public class DefaultLogger : ILogger
{
    public IReadOnlyList<ILogAppender> Appenders { get; }

    private readonly Regex? fileNamePrefixRegex;

    public DefaultLogger(List<ILogAppender>? appenders = null, Regex? fileNamePrefixRegex = null)
    {
        Appenders = new ReadOnlyCollection<ILogAppender>(appenders ?? [new DebugAppender()]);
        this.fileNamePrefixRegex = fileNamePrefixRegex;
    }

    public void Dispose()
    {
        Appenders.ForEach(appender => appender.Dispose());
    }

    public void Debug(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Log(LogLevel.DEBUG, message, exception, file, lineNumber);
    }

    public void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Log(LogLevel.INFO, message, null, file, lineNumber);
    }

    public void Warn(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Log(LogLevel.WARN, message, exception, file, lineNumber);
    }

    public void Error(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Log(LogLevel.ERROR, message, exception, file, lineNumber);
    }

    public void Fatal(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Log(LogLevel.FATAL, message, exception, file, lineNumber);
    }

    private void Log(LogLevel level, string? message, Exception? exception, string file, int lineNumber)
    {
        string timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var fileName = fileNamePrefixRegex is not null ? fileNamePrefixRegex.Replace(file, "") : file;

        string line = ($"{timestamp} | {level} | {fileName}:{lineNumber} | {message ?? exception?.Message ?? ""} \n");

        if (exception != null)
        {
            Append(level, line + exception.ToString());
        }
        else
        {
            Append(level, line);
        }
    }

    private void Append(LogLevel level, string message)
    {
        Appenders.ForEach(appender => appender.Append(level, message));
    }
}
