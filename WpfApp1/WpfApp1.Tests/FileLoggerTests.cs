namespace WpfApp1.Tests;

public class FileLoggerTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly string _logPath;

    public FileLoggerTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), "FileLoggerTests", Guid.NewGuid().ToString());
        _logPath = Path.Combine(_testDirectory, "test.log");
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public void Constructor_CreatesDirectory()
    {
        // Act
        _ = new FileLogger(_logPath);

        // Assert
        Assert.True(Directory.Exists(_testDirectory));
    }

    [Fact]
    public void LogInfo_WritesCorrectFormat()
    {
        // Arrange
        var logger = new FileLogger(_logPath);
        var message = "Test info message";

        // Act
        logger.LogInfo(message);

        // Assert
        var lines = File.ReadAllLines(_logPath);
        Assert.Single(lines);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \[INFO\] Test info message", lines[0]);
    }

    [Fact]
    public void LogError_WritesCorrectFormat()
    {
        // Arrange
        var logger = new FileLogger(_logPath);
        var message = "Test error message";
        var exception = new Exception("Test exception");

        // Act
        logger.LogError(message, exception);

        // Assert
        var lines = File.ReadAllLines(_logPath);
        var expectedPattern = @"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \[ERROR\] Test error message";
        Assert.Matches(expectedPattern, lines[0]);
        Assert.Matches(@"Exception: System.Exception: Test exception", lines[1]);
    }

    [Fact]
    public void LogData_WritesCorrectFormat()
    {
        // Arrange
        var logger = new FileLogger(_logPath);
        var data = new byte[] { 0x01, 0x02, 0x03, 0x04 };
        var direction = "TX";

        // Act
        logger.LogData(direction, data);

        // Assert
        var lines = File.ReadAllLines(_logPath);
        Assert.Single(lines);
        Assert.Matches(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}\.\d{3} \[DATA\] TX: 01-02-03-04", lines[0]);
    }

    [Fact]
    public async Task MultipleThreads_WritesConcurrently()
    {
        // Arrange
        var logger = new FileLogger(_logPath);
        const int threadCount = 10;
        const int messagesPerThread = 100;
        var tasks = new Task[threadCount];

        // Act
        for (var i = 0; i < threadCount; i++)
        {
            var threadId = i;
            tasks[i] = Task.Run(() =>
            {
                for (var j = 0; j < messagesPerThread; j++)
                {
                    logger.LogInfo($"Thread {threadId} Message {j}");
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert
        var lines = File.ReadAllLines(_logPath);
        Assert.Equal(threadCount * messagesPerThread, lines.Length);
        
        // Verify that all messages were written
        var messageCount = lines.Count(line => line.Contains("[INFO]"));
        Assert.Equal(threadCount * messagesPerThread, messageCount);
    }

    [Fact]
    public void EmptyDirectory_WorksCorrectly()
    {
        // Arrange
        var logPath = "test.log"; // No directory part

        // Act
        var logger = new FileLogger(logPath);
        logger.LogInfo("Test message");

        // Assert
        Assert.True(File.Exists(logPath));
        
        // Cleanup
        File.Delete(logPath);
    }
}
