using System;
using System.Threading.Tasks;
using WpfApp1.Connections.Interfaces;
using WpfApp1.Connections.Models;
using WpfApp1.Connections.Logging;

namespace WpfApp1.Services;

public class ConnectionService
{
    private static ConnectionService? _instance;
    public static ConnectionService Instance => _instance ??= new ConnectionService();

    private IConnection? _currentConnection;
    private readonly ILogger _logger;

    public IConnection? CurrentConnection => _currentConnection;
    public IConnectionStatistics? Statistics => _currentConnection?.Statistics;

    public event EventHandler<ConnectionEventArgs>? OnDataReceived;
    public event EventHandler<ConnectionEventArgs>? OnDataSent;
    public event EventHandler<ConnectionEventArgs>? OnError;
    public event EventHandler<bool>? OnConnectionStateChanged;

    public bool IsConnected => _currentConnection != null;

    private ConnectionService()
    {
        _logger = new FileLogger("connection.log");
    }

    public void SetConnection(IConnection connection)
    {
        DisconnectAsync().Wait();
        _currentConnection = connection;
        _currentConnection.OnDataReceived += (s, e) => OnDataReceived?.Invoke(this, e);
        _currentConnection.OnError += (s, e) => OnError?.Invoke(this, e);
    }

    public async Task<bool> ConnectAsync()
    {
        if (_currentConnection == null) return false;

        var result = await _currentConnection.Connect();
        OnConnectionStateChanged?.Invoke(this, result);
        return result;
    }

    public async Task DisconnectAsync()
    {
        if (_currentConnection != null)
        {
            await _currentConnection.DisconnectAsync();
            _currentConnection = null;
            OnConnectionStateChanged?.Invoke(this, false);
        }
    }

    public async Task<bool> SendAsync(byte[] data)
    {
        if (_currentConnection == null) return false;
        var result = await _currentConnection.SendAsync(data);
        if (result)
        {
            OnDataSent?.Invoke(this, new ConnectionEventArgs(data));
        }
        return result;
    }
}
