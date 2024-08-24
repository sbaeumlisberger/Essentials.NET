using System.Text.RegularExpressions;

namespace Essentials.NET.Logging;

public class DefaultLogFormat : ILogFormat
{
    private readonly Regex? filePathPrefixRegex;

    public DefaultLogFormat(Regex? filePathPrefixRegex = null)
    {
        this.filePathPrefixRegex = filePathPrefixRegex;
    }

    public string Format(LogLevel level, string message, Exception? exception, string memberName, string filePath, int lineNumber)
    {
        string timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var fileName = filePathPrefixRegex is not null ? filePathPrefixRegex.Replace(filePath, "") : filePath;

        string line = ($"{timestamp} | {level} | {fileName}:{lineNumber} | {message} \n");

        if (exception != null)
        {
            return line + ExceptionFormatter.Format(exception);
        }
        else
        {
            return line;
        }
    }
}
