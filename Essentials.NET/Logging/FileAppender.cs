using System.Globalization;

namespace Essentials.NET.Logging;

public class FileAppender : ILogAppender
{
    public string LogFilePath { get; }
    public string LogFolderPath { get; }

    internal Task CleanupTask { get; }

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
        int maxFiles = 10,
        TimeSpan? maxAge = default,
        int cleanupDelayInMilliseconds = 5000,
        string logFilePrefix = "log-",
        string logFileDateTimeFormat = "yyyy-MM-dd-HH-mm-ss",
        string logFileExtension = ".txt",
        string logFileExtensionArchive = ".bak",
        TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxFiles, 1, nameof(maxFiles));
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAge ?? TimeSpan.Zero, TimeSpan.Zero, nameof(FileAppender.maxFiles));
        ArgumentOutOfRangeException.ThrowIfLessThan(cleanupDelayInMilliseconds, 0, nameof(cleanupDelayInMilliseconds));

        LogFolderPath = logFolderPath;
        this.level = level;
        this.maxFiles = maxFiles;
        this.maxAge = maxAge;
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
        return Path.Combine(LogFolderPath, fileName);
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
                Directory.EnumerateFiles(LogFolderPath)
                    .Where(filePath => Path.GetFileName(filePath).StartsWith(logFilePrefix))
                    .Where(filePath => File.GetLastWriteTimeUtc(filePath) < cleanupOlderThan)
                    .ForEach(TryDeleteLogFile);
            }

            Directory.EnumerateFiles(LogFolderPath)
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
