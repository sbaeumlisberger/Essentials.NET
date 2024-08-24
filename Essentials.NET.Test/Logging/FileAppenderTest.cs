using Essentials.NET.Logging;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Essentials.NET.Test.Logging;

public class FileAppenderTest
{
    [Fact]
    public async Task Cleanup_MaxFiles()
    {
        var timeProvider = new FakeTimeProvider();

        DirectoryInfo testDirectory = Directory.CreateTempSubdirectory(nameof(FileAppenderTest));

        try
        {
            for (int i = 0; i < 10; i++)
            {
                new FileAppender(testDirectory.FullName, maxCount: 10, timeProvider: timeProvider).Dispose();
                timeProvider.Advance(TimeSpan.FromSeconds(5));
            }

            Assert.Equal(10, testDirectory.GetFiles().Length);

            using var fileAppender = new FileAppender(testDirectory.FullName, maxCount: 10, timeProvider: timeProvider);
            timeProvider.Advance(TimeSpan.FromSeconds(5));
            await fileAppender.CleanupTask;

            Assert.Equal(10, testDirectory.GetFiles().Length);
        }
        finally
        {
            testDirectory.Delete(true);
        }
    }

}
