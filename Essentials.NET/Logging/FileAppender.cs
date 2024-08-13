namespace Essentials.NET.Logging;

public class FileAppender : ILogAppender
{
    private const string LogFilePrefix = "log-";

    private static readonly TimeSpan CleanupDelay = TimeSpan.FromSeconds(5);

    public string LogFilePath { get; private set; }

    internal Task CleanupTask { get; }

    private readonly string logFolderPath;

    private readonly LogLevel level;

    private readonly int maxFiles;

    private readonly TimeSpan maxAge;
    
    private readonly TimeProvider timeProvider;

    private readonly StreamWriter logFileWriter;

    public FileAppender(string logFolderPath, LogLevel level = LogLevel.INFO, int maxFiles = 10, TimeSpan maxAge = default, TimeProvider? timeProvider = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxFiles, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(maxAge, TimeSpan.Zero);

        this.logFolderPath = logFolderPath;
        this.level = level;
        this.maxFiles = maxFiles;
        this.maxAge = maxAge;
        this.timeProvider = timeProvider ?? TimeProvider.System;

        Directory.CreateDirectory(logFolderPath);

        LogFilePath = BuildLogFilePath();
        logFileWriter = CreateLogWriter();

        CleanupTask = Task.Delay(CleanupDelay, this.timeProvider).ContinueWith(_ => CleanupLogFiles());
    }

    public void Dispose()
    {
        logFileWriter.Close();
        logFileWriter.Dispose();
        File.Move(LogFilePath, LogFilePath + ".bak");
    }

    public void Append(LogLevel level, string message)
    {
        if (level >= this.level)
        {
            logFileWriter.Write(message);
        }
    }

    private string BuildLogFilePath()
    {
        return Path.Combine(logFolderPath, LogFilePrefix + timeProvider.GetLocalNow().ToString("yyyy-MM-dd-HH-mm-ss") + ".txt");
    }

    private StreamWriter CreateLogWriter()
    {
        var stream = new FileStream(LogFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read);
        return new StreamWriter(stream) { AutoFlush = true };
    }

    private void CleanupLogFiles()
    {
        try
        {
            if (maxAge != default)
            {
                var cleanupOlderThan = timeProvider.GetUtcNow().Subtract(maxAge);
                Directory.EnumerateFiles(logFolderPath)
                    .Where(filePath => Path.GetFileName(filePath).StartsWith(LogFilePrefix))
                    .Where(filePath => File.GetLastWriteTimeUtc(filePath) < cleanupOlderThan)
                    .ForEach(TryDeleteLogFile);
            }

            Directory.EnumerateFiles(logFolderPath)
                .Where(filePath => Path.GetFileName(filePath).StartsWith(LogFilePrefix))
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .Skip(maxFiles)
                .ForEach(TryDeleteLogFile);
        }
        catch (Exception ex)
        {
            Log.Warn("Failed to cleanup old log files", ex);
        }
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
