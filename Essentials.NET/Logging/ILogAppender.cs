namespace Essentials.NET.Logging;

public interface ILogAppender : IDisposable
{
    void Append(LogLevel level, string message);
}
