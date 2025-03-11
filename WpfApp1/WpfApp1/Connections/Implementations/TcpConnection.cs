using System.Net.Sockets;
using System.Net;
using WpfApp1.Connections.Interfaces;
using WpfApp1.Connections.Models;
using WpfApp1.Connections.Statistics;

namespace WpfApp1.Connections.Implementations;

public class TcpConnection : IConnection
{
    private readonly NetworkSettings _settings;
    private readonly ILogger _logger;
    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _receiveCts;

    public bool IsConnected => _client?.Connected ?? false;
    public int Timeout { get; set; } = 5000;
    public int BufferSize { get; set; } = 1024;
    private readonly ConnectionStatistics _statistics = new();
    public IConnectionStatistics Statistics => _statistics;

    public event EventHandler<ConnectionEventArgs>? OnDataReceived;
    public event EventHandler<ConnectionEventArgs>? OnError;

    public TcpConnection(NetworkSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<bool> Connect()
    {
        try
        {
            _client = new TcpClient();
            _client.ReceiveTimeout = Timeout;
            _client.SendTimeout = Timeout;

            if (_settings.LocalAddress != null)
            {
                await Task.Run(() => _client.Client.Bind(new IPEndPoint(_settings.LocalAddress, 0)));
            }

            var ipAddress = IPAddress.Parse(_settings.IpAddress);
            await _client.ConnectAsync(ipAddress, _settings.Port);
            _stream = _client.GetStream();

            _receiveCts = new CancellationTokenSource();
            _ = StartReceiving(_receiveCts.Token);

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
            _stream?.Close();
            _client?.Close();
            _client?.Dispose();
            _client = null;
            _stream = null;
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
        if (!IsConnected || _stream == null)
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
            await _stream.WriteAsync(data);
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
        var buffer = new byte[BufferSize];

        while (!cancellationToken.IsCancellationRequested && _stream != null)
        {
            try
            {
                var bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0) // Connection closed by peer
                {
                    break;
                }

                var receivedData = new byte[bytesRead];
                Array.Copy(buffer, receivedData, bytesRead);

                await Task.Run(() =>
                {
                    ((ConnectionStatistics)Statistics).AddReceivedBytes(bytesRead);
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
