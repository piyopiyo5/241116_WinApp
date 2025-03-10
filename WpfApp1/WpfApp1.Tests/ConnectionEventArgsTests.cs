namespace WpfApp1.Tests;

public class ConnectionEventArgsTests
{
    [Fact]
    public void Constructor_WithData_InitializesCorrectly()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var message = "Test message";

        // Act
        var args = new ConnectionEventArgs(data, message);

        // Assert
        Assert.Equal(data, args.Data);
        Assert.Equal(message, args.Message);
        Assert.Null(args.Error);
        Assert.True((DateTime.Now - args.Timestamp).TotalSeconds < 1);
    }

    [Fact]
    public void Constructor_WithError_InitializesCorrectly()
    {
        // Arrange
        var error = new Exception("Test error");
        var message = "Error message";

        // Act
        var args = new ConnectionEventArgs(message: message, error: error);

        // Assert
        Assert.Null(args.Data);
        Assert.Equal(message, args.Message);
        Assert.Equal(error, args.Error);
        Assert.True((DateTime.Now - args.Timestamp).TotalSeconds < 1);
    }

    [Fact]
    public void Constructor_WithAllParameters_InitializesCorrectly()
    {
        // Arrange
        var data = new byte[] { 1, 2, 3 };
        var message = "Test message with data and error";
        var error = new Exception("Test error");

        // Act
        var args = new ConnectionEventArgs(data, message, error);

        // Assert
        Assert.Equal(data, args.Data);
        Assert.Equal(message, args.Message);
        Assert.Equal(error, args.Error);
        Assert.True((DateTime.Now - args.Timestamp).TotalSeconds < 1);
    }

    [Fact]
    public void Constructor_WithDefaults_InitializesCorrectly()
    {
        // Act
        var args = new ConnectionEventArgs();

        // Assert
        Assert.Null(args.Data);
        Assert.Equal(string.Empty, args.Message);
        Assert.Null(args.Error);
        Assert.True((DateTime.Now - args.Timestamp).TotalSeconds < 1);
    }
}
