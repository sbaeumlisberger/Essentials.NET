namespace Essentials.NET.Logging;

public interface ILogFormat
{
    string Format(LogLevel level, string message, Exception? exception, string memberName, string filePath, int lineNumber);
}
