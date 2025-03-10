namespace WpfApp1.Tests;

public class ConnectionStatisticsTests
{
    [Fact]
    public void Constructor_InitializesWithDefaultValues()
    {
        // Arrange & Act
        var stats = new ConnectionStatistics();

        // Assert
        Assert.Equal(0, stats.BytesSent);
        Assert.Equal(0, stats.BytesReceived);
        Assert.Equal(0, stats.ErrorCount);
        Assert.Equal(DateTime.MinValue, stats.LastConnected);
        Assert.Equal(DateTime.MinValue, stats.LastError);
        Assert.True(stats.Uptime.TotalSeconds < 1); // Should be very recent
    }

    [Fact]
    public void AddSentBytes_IncrementsCounter()
    {
        // Arrange
        var stats = new ConnectionStatistics();

        // Act
        stats.AddSentBytes(100);
        stats.AddSentBytes(50);

        // Assert
        Assert.Equal(150, stats.BytesSent);
    }

    [Fact]
    public void AddReceivedBytes_IncrementsCounter()
    {
        // Arrange
        var stats = new ConnectionStatistics();

        // Act
        stats.AddReceivedBytes(200);
        stats.AddReceivedBytes(75);

        // Assert
        Assert.Equal(275, stats.BytesReceived);
    }

    [Fact]
    public void IncrementErrorCount_UpdatesCountAndTimestamp()
    {
        // Arrange
        var stats = new ConnectionStatistics();
        var beforeError = DateTime.Now;

        // Act
        stats.IncrementErrorCount();
        var afterError = DateTime.Now;

        // Assert
        Assert.Equal(1, stats.ErrorCount);
        Assert.True(stats.LastError >= beforeError);
        Assert.True(stats.LastError <= afterError);
    }

    [Fact]
    public void UpdateLastConnected_SetsTimestamp()
    {
        // Arrange
        var stats = new ConnectionStatistics();
        var beforeUpdate = DateTime.Now;

        // Act
        stats.UpdateLastConnected();
        var afterUpdate = DateTime.Now;

        // Assert
        Assert.True(stats.LastConnected >= beforeUpdate);
        Assert.True(stats.LastConnected <= afterUpdate);
    }

    [Fact]
    public void Reset_ResetsAllValues()
    {
        // Arrange
        var stats = new ConnectionStatistics();
        stats.AddSentBytes(100);
        stats.AddReceivedBytes(200);
        stats.IncrementErrorCount();
        stats.UpdateLastConnected();

        // Act
        stats.Reset();

        // Assert
        Assert.Equal(0, stats.BytesSent);
        Assert.Equal(0, stats.BytesReceived);
        Assert.Equal(0, stats.ErrorCount);
        Assert.Equal(DateTime.MinValue, stats.LastConnected);
        Assert.Equal(DateTime.MinValue, stats.LastError);
    }

    [Fact]
    public void AddSentBytes_ThreadSafe()
    {
        // Arrange
        var stats = new ConnectionStatistics();
        const int threadCount = 10;
        const int bytesPerThread = 1000;
        var threads = new Thread[threadCount];

        // Act
        for (var i = 0; i < threadCount; i++)
        {
            threads[i] = new Thread(() =>
            {
                for (var j = 0; j < bytesPerThread; j++)
                {
                    stats.AddSentBytes(1);
                }
            });
            threads[i].Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // Assert
        Assert.Equal(threadCount * bytesPerThread, stats.BytesSent);
    }
}
