using System.IO.Ports;
using WpfApp1.Connections.Interfaces;
using WpfApp1.Connections.Models;
using WpfApp1.Connections.Statistics;

namespace WpfApp1.Connections.Implementations;

public class SerialConnection : IConnection
{
    private readonly SerialSettings _settings;
    private readonly ILogger _logger;
    private SerialPort? _port;
    private CancellationTokenSource? _receiveCts;
    private bool _isConnected;

    public bool IsConnected => _isConnected;
    public int Timeout { get; set; } = 5000;
    public int BufferSize { get; set; } = 1024;
    private readonly ConnectionStatistics _statistics = new();
    public IConnectionStatistics Statistics => _statistics;

    public event EventHandler<ConnectionEventArgs>? OnDataReceived;
    public event EventHandler<ConnectionEventArgs>? OnError;

    public SerialConnection(SerialSettings settings, ILogger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public async Task<bool> Connect()
    {
        try
        {
            _port = new SerialPort
            {
                PortName = _settings.PortName,
                BaudRate = _settings.BaudRate,
                Parity = _settings.Parity,
                DataBits = _settings.DataBits,
                StopBits = _settings.StopBits,
                RtsEnable = _settings.RtsEnable,
                DtrEnable = _settings.DtrEnable,
                ReadTimeout = Timeout,
                WriteTimeout = Timeout
            };

            await Task.Run(() => _port.Open());
            _receiveCts = new CancellationTokenSource();
            _ = StartReceiving(_receiveCts.Token);

            _isConnected = true;
            await Task.Run(() =>
            {
                ((ConnectionStatistics)Statistics).UpdateLastConnected();
                _logger.LogInfo($"Connected to {_settings.PortName}");
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
            if (_port?.IsOpen == true)
            {
                await Task.Run(() => _port.Close());
            }
            await Task.Run(() =>
            {
                _port?.Dispose();
                _port = null;
                _isConnected = false;
                _logger.LogInfo("Disconnected");
            });
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
        if (!IsConnected || _port?.IsOpen != true)
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
            await _port.BaseStream.WriteAsync(data);
            await _port.BaseStream.FlushAsync();
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

        while (!cancellationToken.IsCancellationRequested && _port?.IsOpen == true)
        {
            try
            {
                var bytesRead = await _port.BaseStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                if (bytesRead == 0)
                {
                    continue;
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
