using System.Globalization;

namespace Essentials.NET.Logging;

public class FileAppender : ILogAppender
{
    public string LogFilePath { get; private set; }

    internal Task CleanupTask { get; }

    protected readonly string logFolderPath;
    protected readonly LogLevel level;
    private readonly int maxFiles;
    private readonly TimeSpan? maxAge;
    private readonly TimeSpan cleanupDelay;
    private readonly string logFilePrefix;
    private readonly string logFileDateTimeFormat;
    private readonly string logFileExtension;
    private readonly string logFileExtensionArchive;
    private readonly TimeProvider timeProvider;

    private readonly TextWriter logFileWriter;

    public FileAppender(
        string logFolderPath,
        LogLevel level = LogLevel.INFO,
        int maxCount = 10,
        TimeSpan? maxFiles = default,
        int cleanupDelayInMilliseconds = 5000,
        string logFilePrefix = "log-",
        string logFileDateTimeFormat = "yyyy-MM-dd-HH-mm-ss",
        string logFileExtension = ".txt",
        string logFileExtensionArchive = ".bak",
        TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxCount, 1, nameof(maxCount));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxFiles ?? TimeSpan.Zero, TimeSpan.Zero, nameof(maxFiles));
        ArgumentOutOfRangeException.ThrowIfLessThan(cleanupDelayInMilliseconds, 0, nameof(cleanupDelayInMilliseconds));

        this.logFolderPath = logFolderPath;
        this.level = level;
        this.maxFiles = maxCount;
        this.maxAge = maxFiles;
        this.cleanupDelay = TimeSpan.FromMilliseconds(cleanupDelayInMilliseconds);
        this.logFilePrefix = logFilePrefix;
        this.logFileDateTimeFormat = logFileDateTimeFormat;
        this.logFileExtension = logFileExtension;
        this.logFileExtensionArchive = logFileExtensionArchive;
        this.timeProvider = timeProvider ?? TimeProvider.System;

        Directory.CreateDirectory(logFolderPath);

        LogFilePath = BuildLogFilePath();
        logFileWriter = CreateLogWriter();

        CleanupTask = Task.Delay(cleanupDelay, this.timeProvider).ContinueWith(_ => CleanupLogFiles());
    }

    public void Dispose()
    {
        logFileWriter.Close();
        logFileWriter.Dispose();
        ArchiveLogFile();
    }

    public void Append(LogLevel level, string message)
    {
        if (level >= this.level)
        {
            logFileWriter.Write(message);
        }
    }

    protected virtual string BuildLogFilePath()
    {
        string formattedDateTime = timeProvider.GetLocalNow().ToString(logFileDateTimeFormat, CultureInfo.InvariantCulture);
        string fileName = logFilePrefix + formattedDateTime + logFileExtension;
        return Path.Combine(logFolderPath, fileName);
    }

    protected virtual void ArchiveLogFile()
    {
        File.Move(LogFilePath, LogFilePath + logFileExtensionArchive);
    }

    protected virtual void CleanupLogFiles()
    {
        try
        {
            if (maxAge != null)
            {
                var cleanupOlderThan = timeProvider.GetUtcNow().Subtract(maxAge.Value);
                Directory.EnumerateFiles(logFolderPath)
                    .Where(filePath => Path.GetFileName(filePath).StartsWith(logFilePrefix))
                    .Where(filePath => File.GetLastWriteTimeUtc(filePath) < cleanupOlderThan)
                    .ForEach(TryDeleteLogFile);
            }

            Directory.EnumerateFiles(logFolderPath)
                .Where(filePath => Path.GetFileName(filePath).StartsWith(logFilePrefix))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .Skip(maxFiles)
                .ForEach(TryDeleteLogFile);
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to cleanup old log files", ex);
        }
    }

    private TextWriter CreateLogWriter()
    {
        var stream = new FileStream(LogFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        return TextWriter.Synchronized(new StreamWriter(stream) { AutoFlush = true });
    }

    private void TryDeleteLogFile(string filePath)
    {
        try
        {
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            Log.Warn($"Failed to delete log file \"{Path.GetFileName(filePath)}\"", ex);
        }
    }
}
