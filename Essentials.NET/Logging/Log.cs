using System.Runtime.CompilerServices;

namespace Essentials.NET.Logging;

public static class Log
{
    public static ILogger Logger { get; private set; } = new DefaultLogger();

    public static void Configure(ILogger logger)
    {
        Logger.Dispose();
        Logger = logger;
    }

    public static void Debug(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Debug(message, exception, memberName, file, lineNumber);
    }

    public static void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Info(message, memberName, file, lineNumber);
    }

    public static void Warn(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Warn(message, exception, memberName, file, lineNumber);
    }

    public static void Error(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Error(message, exception, memberName, file, lineNumber);
    }

    public static void Fatal(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        Logger.Fatal(message, exception, memberName, file, lineNumber);
    }
}
