namespace WpfApp1.Tests;

public class TcpConnectionTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly NetworkSettings _settings;

    public TcpConnectionTests()
    {
        _mockLogger = new Mock<ILogger>();
        _settings = new NetworkSettings
        {
            IpAddress = IPAddress.Loopback.ToString(),
            Port = 12345,
            LocalAddress = IPAddress.Any
        };
    }

    [Fact]
    public void Constructor_InitializesCorrectly()
    {
        // Act
        var connection = new TcpConnection(_settings, _mockLogger.Object);

        // Assert
        Assert.NotNull(connection.Statistics);
        Assert.False(connection.IsConnected);
        Assert.Equal(5000, connection.Timeout);
        Assert.Equal(1024, connection.BufferSize);
    }

    [Fact]
    public async Task Connect_Successful_UpdatesStatistics()
    {
        // Arrange
        var connection = new TcpConnection(_settings, _mockLogger.Object);

        // Listenerを起動してTCP接続を待ち受ける
        var listener = new TcpListener(IPAddress.Loopback, _settings.Port);
        listener.Start();

        try
        {
            // Act
            var result = await connection.Connect();

            // Assert
            Assert.True(result);
            Assert.True(connection.IsConnected);
            _mockLogger.Verify(l => l.LogInfo(It.IsAny<string>()), Times.Once);
        }
        finally
        {
            await connection.DisconnectAsync();
            listener.Stop();
        }
    }

    [Fact]
    public async Task Connect_Failed_UpdatesStatistics()
    {
        // Arrange
        var settings = new NetworkSettings
        {
            IpAddress = IPAddress.Loopback.ToString(),
            Port = 54321 // 使用されていないポート
        };
        var connection = new TcpConnection(settings, _mockLogger.Object);
        var errorOccurred = false;
        connection.OnError += (sender, args) => errorOccurred = true;

        // Act
        var result = await connection.Connect();

        // Assert
        Assert.False(result);
        Assert.False(connection.IsConnected);
        Assert.True(errorOccurred);
        _mockLogger.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_WhenNotConnected_ReturnsFalse()
    {
        // Arrange
        var connection = new TcpConnection(_settings, _mockLogger.Object);
        var errorOccurred = false;
        connection.OnError += (sender, args) => errorOccurred = true;

        // Act
        var result = await connection.SendAsync(new byte[] { 1, 2, 3 });

        // Assert
        Assert.False(result);
        Assert.True(errorOccurred);
    }

    [Fact]
    public async Task SendAsync_WhenConnected_SendsData()
    {
        // Arrange
        var connection = new TcpConnection(_settings, _mockLogger.Object);
        var listener = new TcpListener(IPAddress.Loopback, _settings.Port);
        listener.Start();

        try
        {
            var connectionTask = connection.Connect();
            var client = await listener.AcceptTcpClientAsync();
            await connectionTask;

            var testData = new byte[] { 1, 2, 3, 4 };

            // Act
            var result = await connection.SendAsync(testData);

            // Verify the data was received
            var buffer = new byte[1024];
            var bytesRead = await client.GetStream().ReadAsync(buffer, 0, buffer.Length);
            var receivedData = new byte[bytesRead];
            Array.Copy(buffer, receivedData, bytesRead);

            // Assert
            Assert.True(result);
            Assert.Equal(testData, receivedData);
            _mockLogger.Verify(l => l.LogData("Sent", It.IsAny<byte[]>()), Times.Once);
        }
        finally
        {
            await connection.DisconnectAsync();
            listener.Stop();
        }
    }

    [Fact]
    public async Task DataReceived_RaisesEvent()
    {
        // Arrange
        var connection = new TcpConnection(_settings, _mockLogger.Object);
        var listener = new TcpListener(IPAddress.Loopback, _settings.Port);
        listener.Start();

        byte[]? receivedData = null;
        connection.OnDataReceived += (sender, args) => receivedData = args.Data;

        try
        {
            var connectionTask = connection.Connect();
            var client = await listener.AcceptTcpClientAsync();
            await connectionTask;

            var testData = new byte[] { 1, 2, 3, 4 };

            // Act
            await client.GetStream().WriteAsync(testData, 0, testData.Length);

            // Wait for data to be processed
            await Task.Delay(100);

            // Assert
            Assert.NotNull(receivedData);
            Assert.Equal(testData, receivedData);
            _mockLogger.Verify(l => l.LogData("Received", It.IsAny<byte[]>()), Times.Once);
        }
        finally
        {
            await connection.DisconnectAsync();
            listener.Stop();
        }
    }

    [Fact]
    public async Task DisconnectAsync_ClosesConnection()
    {
        // Arrange
        var connection = new TcpConnection(_settings, _mockLogger.Object);
        var listener = new TcpListener(IPAddress.Loopback, _settings.Port);
        listener.Start();

        try
        {
            await connection.Connect();

            // Act
            await connection.DisconnectAsync();

            // Assert
            Assert.False(connection.IsConnected);
            _mockLogger.Verify(l => l.LogInfo("Disconnected"), Times.Once);
        }
        finally
        {
            listener.Stop();
        }
    }
}
