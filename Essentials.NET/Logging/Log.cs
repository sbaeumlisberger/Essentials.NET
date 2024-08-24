using System.Runtime.CompilerServices;

namespace Essentials.NET.Logging;

public static class Log
{
    public static ILogger Logger { get; private set; } = new Logger();

    public static void Configure(ILogger logger)
    {
        Logger.Dispose();
        Logger = logger;
    }

    public static void Debug(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Debug(message, exception, memberName, filePath, lineNumber);
    }

    public static void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Info(message, memberName, filePath, lineNumber);
    }

    public static void Warn(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Warn(message, exception, memberName, filePath, lineNumber);
    }

    public static void Error(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Error(message, exception, memberName, filePath, lineNumber);
    }

    public static void Fatal(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Fatal(message, exception, memberName, filePath, lineNumber);
    }

    public static void Write(LogLevel level, string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Write(level, message, exception, memberName, filePath, lineNumber);
    }

    public static void DisposeLogger()
    {
        Logger.Dispose();
    }
}
