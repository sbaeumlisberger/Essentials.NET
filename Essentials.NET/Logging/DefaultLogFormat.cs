using System.Globalization;
using System.Text.RegularExpressions;

namespace Essentials.NET.Logging;

public class DefaultLogFormat : ILogFormat
{
    internal TimeProvider TimeProvider { get; init; } = TimeProvider.System;
    internal Func<int> ThreadIdProvider { get; init; } = () => Environment.CurrentManagedThreadId;

    private readonly Regex? filePathPrefixRegex;
    private readonly int? filePathOffset;

    public DefaultLogFormat(Regex? filePathPrefixRegex = null, int? filePathOffset = null)
    {
        this.filePathPrefixRegex = filePathPrefixRegex;
        this.filePathOffset = filePathOffset;
    }

    public string Format(LogLevel level, string message, Exception? exception, string memberName, string filePath, int lineNumber)
    {
        string timestamp = TimeProvider.GetLocalNow().ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
        var shortFilePath = GetShortFilePath(filePath);
        int threadId = ThreadIdProvider.Invoke();

        string line = $"{timestamp} | {level,-5} | {threadId,3} | {shortFilePath}:{lineNumber} | {message} \n";

        if (exception != null)
        {
            return line + ExceptionFormatter.Format(exception);
        }
        else
        {
            return line;
        }
    }

    private ReadOnlySpan<char> GetShortFilePath(string filePath)
    {
        if (filePathOffset is not null)
        {
            return filePath.AsSpan().Slice(filePathOffset.Value);
        }
        else if(filePathPrefixRegex is not null)
        {
            return filePathPrefixRegex.Replace(filePath, "");
        }
        else
        {
            return filePath;
        }
    }
}
