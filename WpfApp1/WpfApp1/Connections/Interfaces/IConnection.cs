using WpfApp1.Connections.Models;

namespace WpfApp1.Connections.Interfaces;

public interface IConnection
{
    bool IsConnected { get; }
    int Timeout { get; set; }
    int BufferSize { get; set; }
    IConnectionStatistics Statistics { get; }

    event EventHandler<ConnectionEventArgs> OnDataReceived;
    event EventHandler<ConnectionEventArgs> OnError;

    Task<bool> Connect();
    Task DisconnectAsync();
    Task<bool> SendAsync(byte[] data);
}
