namespace Essentials.NET.Logging;

public class ConsoleAppender : ILogAppender
{
    public void Append(LogLevel level, string message)
    {
        Console.Write(message);
    }

    public void Dispose()
    {
        // not needed
    }
}
