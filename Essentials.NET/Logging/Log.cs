using System.Runtime.CompilerServices;

namespace Essentials.NET.Logging;

public static class Log
{
    private static ILogger logger = new DefaultLogger();

    public static void Configure(ILogger logger)
    {
        Log.logger.Dispose();
        Log.logger = logger;
    }

    public static void Debug(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        logger.Debug(message, exception, memberName, file, lineNumber);
    }

    public static void Info(string message, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        logger.Info(message, memberName, file, lineNumber);
    }

    public static void Warn(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        logger.Warn(message, exception, memberName, file, lineNumber);
    }

    public static void Error(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        logger.Error(message, exception, memberName, file, lineNumber);
    }

    public static void Fatal(string message, Exception? exception = null, [CallerMemberName] string memberName = "", [CallerFilePath] string file = "", [CallerLineNumber] int lineNumber = -1)
    {
        logger.Fatal(message, exception, memberName, file, lineNumber);
    }
}
