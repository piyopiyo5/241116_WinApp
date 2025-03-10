using WpfApp1.Connections.Interfaces;

namespace WpfApp1.Connections.Statistics;

public class ConnectionStatistics : IConnectionStatistics
{
    private long _bytesSent;
    private long _bytesReceived;
    private int _errorCount;
    private readonly DateTime _startTime;
    private DateTime _lastConnected;
    private DateTime _lastError;

    public long BytesSent => _bytesSent;
    public long BytesReceived => _bytesReceived;
    public int ErrorCount => _errorCount;
    public TimeSpan Uptime => DateTime.Now - _startTime;
    public DateTime LastConnected => _lastConnected;
    public DateTime LastError => _lastError;

    public ConnectionStatistics()
    {
        _startTime = DateTime.Now;
        _lastConnected = DateTime.MinValue;
        _lastError = DateTime.MinValue;
    }

    public void AddSentBytes(long bytes)
    {
        Interlocked.Add(ref _bytesSent, bytes);
    }

    public void AddReceivedBytes(long bytes)
    {
        Interlocked.Add(ref _bytesReceived, bytes);
    }

    public void IncrementErrorCount()
    {
        Interlocked.Increment(ref _errorCount);
        _lastError = DateTime.Now;
    }

    public void UpdateLastConnected()
    {
        _lastConnected = DateTime.Now;
    }

    public void Reset()
    {
        _bytesSent = 0;
        _bytesReceived = 0;
        _errorCount = 0;
        _lastConnected = DateTime.MinValue;
        _lastError = DateTime.MinValue;
    }
}
