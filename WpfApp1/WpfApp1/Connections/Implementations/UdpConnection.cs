using System.Net;
using System.Net.Sockets;
using WpfApp1.Connections.Interfaces;
using WpfApp1.Connections.Models;
using WpfApp1.Connections.Statistics;

namespace WpfApp1.Connections.Implementations;

public class UdpConnection : IConnection
{
    private readonly NetworkSettings _settings;
    private readonly ILogger _logger;
    private UdpClient? _client;
    private CancellationTokenSource? _receiveCts;
    private bool _isConnected;

    public bool IsConnected => _isConnected;
    public int Timeout { get; set; } = 5000;
    public int BufferSize { get; set; } = 1024;
    public IConnectionStatistics Statistics { get; }

    public event EventHandler<ConnectionEventArgs>? OnDataReceived;
    public event EventHandler<ConnectionEventArgs>? OnError;

    public UdpConnection(NetworkSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
        Statistics = new ConnectionStatistics();
    }

    public async Task<bool> Connect()
    {
        try
        {
            _client = new UdpClient();
            if (_settings.LocalAddress != null)
            {
                await Task.Run(() => _client.Client.Bind(new IPEndPoint(_settings.LocalAddress, 0)));
            }
            
            await Task.Run(() => _client.Connect(_settings.IpAddress, _settings.Port));
            _client.Client.ReceiveTimeout = Timeout;
            _client.Client.SendTimeout = Timeout;

            _receiveCts = new CancellationTokenSource();
            _ = StartReceiving(_receiveCts.Token);

            _isConnected = true;
            await Task.Run(() =>
            {
                ((ConnectionStatistics)Statistics).UpdateLastConnected();
                _logger.LogInfo($"Connected to {_settings.IpAddress}:{_settings.Port}");
            });
            return true;
        }
        catch (Exception ex)
        {
            await Task.Run(() =>
            {
                ((ConnectionStatistics)Statistics).IncrementErrorCount();
                OnError?.Invoke(this, new ConnectionEventArgs(error: ex));
                _logger.LogError("Connection failed", ex);
            });
            return false;
        }
    }

    public async Task DisconnectAsync()
    {
        try
        {
            _receiveCts?.Cancel();
            _client?.Close();
            _client?.Dispose();
            _client = null;
            _isConnected = false;
            await Task.Run(() => _logger.LogInfo("Disconnected"));
        }
        catch (Exception ex)
        {
            await Task.Run(() =>
            {
                ((ConnectionStatistics)Statistics).IncrementErrorCount();
                OnError?.Invoke(this, new ConnectionEventArgs(error: ex));
                _logger.LogError("Disconnect failed", ex);
            });
        }
    }

    public async Task<bool> SendAsync(byte[] data)
    {
        if (!IsConnected || _client == null)
        {
            var ex = new InvalidOperationException("Not connected");
            await Task.Run(() =>
            {
                ((ConnectionStatistics)Statistics).IncrementErrorCount();
                OnError?.Invoke(this, new ConnectionEventArgs(error: ex));
            });
            return false;
        }

        try
        {
            await _client.SendAsync(data);
            await Task.Run(() =>
            {
                ((ConnectionStatistics)Statistics).AddSentBytes(data.Length);
                _logger.LogData("Sent", data);
            });
            return true;
        }
        catch (Exception ex)
        {
            await Task.Run(() =>
            {
                ((ConnectionStatistics)Statistics).IncrementErrorCount();
                OnError?.Invoke(this, new ConnectionEventArgs(error: ex));
                _logger.LogError("Send failed", ex);
            });
            return false;
        }
    }

    private async Task StartReceiving(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _client != null)
        {
            try
            {
                var result = await _client.ReceiveAsync(cancellationToken);
                var receivedData = result.Buffer;

                await Task.Run(() =>
                {
                    ((ConnectionStatistics)Statistics).AddReceivedBytes(receivedData.Length);
                    _logger.LogData("Received", receivedData);
                    OnDataReceived?.Invoke(this, new ConnectionEventArgs(receivedData));
                });
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                await Task.Run(() =>
                {
                    ((ConnectionStatistics)Statistics).IncrementErrorCount();
                    OnError?.Invoke(this, new ConnectionEventArgs(error: ex));
                    _logger.LogError("Receive failed", ex);
                });
                break;
            }
        }
    }
}
