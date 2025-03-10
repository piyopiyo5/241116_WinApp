namespace WpfApp1.Connections.Interfaces;

public interface IConnectionStatistics
{
    long BytesSent { get; }
    long BytesReceived { get; }
    int ErrorCount { get; }
    TimeSpan Uptime { get; }
    DateTime LastConnected { get; }
    DateTime LastError { get; }

    void Reset();
}
