namespace WpfApp1.Tests;

public class UdpConnectionTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly NetworkSettings _settings;

    public UdpConnectionTests()
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
        var connection = new UdpConnection(_settings, _mockLogger.Object);

        // Assert
        Assert.NotNull(connection.Statistics);
        Assert.False(connection.IsConnected);
        Assert.Equal(1024, connection.BufferSize);
    }

    [Fact]
    public async Task Connect_InitializesUdpClient()
    {
        // Arrange
        var connection = new UdpConnection(_settings, _mockLogger.Object);

        // Act
        var result = await connection.Connect();

        try
        {
            // Assert
            Assert.True(result);
            Assert.True(connection.IsConnected);
            _mockLogger.Verify(l => l.LogInfo(It.IsAny<string>()), Times.Once);
        }
        finally
        {
            await connection.DisconnectAsync();
        }
    }

    [Fact]
    public async Task SendAsync_WhenNotConnected_ReturnsFalse()
    {
        // Arrange
        var connection = new UdpConnection(_settings, _mockLogger.Object);
        var errorOccurred = false;
        connection.OnError += (sender, args) => errorOccurred = true;

        // Act
        var result = await connection.SendAsync(new byte[] { 1, 2, 3 });

        // Assert
        Assert.False(result);
        Assert.True(errorOccurred);
    }

    [Fact]
    public async Task SendAndReceive_WorksCorrectly()
    {
        // Arrange
        const int testPort = 12346;
        
        // 受信側の設定: ローカルホストの特定ポートにバインド
        var receiverSettings = new NetworkSettings 
        { 
            IpAddress = IPAddress.Loopback.ToString(),
            Port = testPort,
            LocalAddress = IPAddress.Loopback
        };

        // 送信側の設定: ローカルホストの特定ポートに送信
        var senderSettings = new NetworkSettings
        {
            IpAddress = IPAddress.Loopback.ToString(),
            Port = testPort,
            LocalAddress = IPAddress.Any
        };

        var receiver = new UdpConnection(receiverSettings, _mockLogger.Object);
        var sender = new UdpConnection(senderSettings, _mockLogger.Object);

        byte[]? receivedData = null;
        receiver.OnDataReceived += (sender, args) => receivedData = args.Data;

        try
        {
            // まず受信側を起動
            var receiverConnected = await receiver.Connect();
            Assert.True(receiverConnected, "Receiver failed to connect");

            // 少し待機して受信側の準備を確実に
            await Task.Delay(100);

            // 送信側を接続
            var senderConnected = await sender.Connect();
            Assert.True(senderConnected, "Sender failed to connect");

            var testData = new byte[] { 1, 2, 3, 4 };

            // Act
            var sendResult = await sender.SendAsync(testData);
            Assert.True(sendResult, "Failed to send data");

            // より長い待機時間を設定
            await Task.Delay(1000);

            // Assert
            Assert.NotNull(receivedData);
            Assert.Equal(testData, receivedData);
            _mockLogger.Verify(l => l.LogData("Sent", It.IsAny<byte[]>()), Times.Once);
            _mockLogger.Verify(l => l.LogData("Received", It.IsAny<byte[]>()), Times.Once);
        }
        finally
        {
            await receiver.DisconnectAsync();
            await sender.DisconnectAsync();
        }
    }

    [Fact]
    public async Task DisconnectAsync_ClosesConnection()
    {
        // Arrange
        var connection = new UdpConnection(_settings, _mockLogger.Object);
        await connection.Connect();

        // Act
        await connection.DisconnectAsync();

        // Assert
        Assert.False(connection.IsConnected);
        _mockLogger.Verify(l => l.LogInfo("Disconnected"), Times.Once);
    }
}
