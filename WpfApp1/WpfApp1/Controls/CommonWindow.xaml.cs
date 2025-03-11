using System;
using System.Linq;
using System.Text;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
using WpfApp1.Connections.Implementations;
using WpfApp1.Connections.Interfaces;
using WpfApp1.Connections.Logging;
using WpfApp1.Connections.Models;
using WpfApp1.Services;

namespace WpfApp1.Controls
{
    /// <summary>
    /// CommonWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class CommonWindow : UserControl
    {
    private readonly ConnectionService _connectionService;
    private readonly ILogger _logger;
    private readonly DispatcherTimer _statisticsTimer;

    public CommonWindow()
    {
        InitializeComponent();
        _connectionService = ConnectionService.Instance;
        _logger = new FileLogger("connection.log");

        // イベントハンドラの設定
        _connectionService.OnDataReceived += Connection_OnDataReceived;
        _connectionService.OnDataSent += Connection_OnDataSent;
        _connectionService.OnError += Connection_OnError;
        _connectionService.OnConnectionStateChanged += Connection_OnConnectionStateChanged;

        // 初期化
        // 統計情報更新タイマーの設定
        _statisticsTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _statisticsTimer.Tick += StatisticsTimer_Tick;

        InitializeUI();
        CommunicationTypeComboBox.SelectedIndex = 0;
    }

    private void InitializeUI()
    {
        // NICリストの取得
        var interfaces = NetworkSettings.GetAvailableInterfaces();
        var interfaceItems = interfaces
            .Select(ni => new { Name = ni.Name, Interface = ni })
            .ToList();

        NetworkInterfaceComboBox.ItemsSource = interfaceItems;
        NetworkInterfaceComboBox.DisplayMemberPath = "Name";
        if (interfaceItems.Any())
        {
            NetworkInterfaceComboBox.SelectedIndex = 0;
        }

        // シリアルポートの取得
        var ports = SerialSettings.GetAvailablePorts();
        SerialPortComboBox.ItemsSource = ports;
        if (ports.Length > 0)
        {
            SerialPortComboBox.SelectedIndex = 0;
        }

        // ボーレートの初期設定
        BaudRateComboBox.SelectedIndex = 0;
    }

    private void CommunicationTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var isSerial = CommunicationTypeComboBox.SelectedIndex == 2;

        // TCP/UDP用の設定
        var tcpUdpVisibility = !isSerial ? Visibility.Visible : Visibility.Collapsed;
        NetworkInterfaceLabel.Visibility = tcpUdpVisibility;
        NetworkInterfaceComboBox.Visibility = tcpUdpVisibility;
        AddressLabel.Visibility = tcpUdpVisibility;
        AddressTextBox.Visibility = tcpUdpVisibility;
        PortLabel.Visibility = tcpUdpVisibility;
        PortTextBox.Visibility = tcpUdpVisibility;

        // シリアル用の設定
        var serialVisibility = isSerial ? Visibility.Visible : Visibility.Collapsed;
        SerialPortLabel.Visibility = serialVisibility;
        SerialPortComboBox.Visibility = serialVisibility;
        BaudRateLabel.Visibility = serialVisibility;
        BaudRateComboBox.Visibility = serialVisibility;
    }

    private async void ConnectButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (CommunicationTypeComboBox.SelectedIndex == 2) // Serial
            {
                var settings = new SerialSettings(
                    SerialPortComboBox.SelectedItem?.ToString() ?? "",
                    int.Parse(((ComboBoxItem)BaudRateComboBox.SelectedItem).Content.ToString() ?? "9600")
                );
                _connectionService.SetConnection(new SerialConnection(settings, _logger));
            }
            else // TCP or UDP
            {
                if (!int.TryParse(PortTextBox.Text, out var port))
                {
                    MessageBox.Show("ポート番号が正しくありません");
                    return;
                }

                var selectedItem = (dynamic)NetworkInterfaceComboBox.SelectedItem;
                var selectedInterface = selectedItem?.Interface as NetworkInterface;
                var localAddress = selectedInterface?.GetIPProperties().UnicastAddresses
                    .First(a => a.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    .Address;

                var settings = new NetworkSettings(AddressTextBox.Text, port, localAddress);

                _connectionService.SetConnection(
                    CommunicationTypeComboBox.SelectedIndex == 0
                        ? new TcpConnection(settings, _logger)
                        : new UdpConnection(settings, _logger)
                );
            }

            if (await _connectionService.ConnectAsync())
            {
                LogMessage("接続しました");
            }
            else
            {
                LogMessage("接続に失敗しました");
            }
        }
        catch (Exception ex)
        {
            LogMessage($"エラー: {ex.Message}");
        }
    }

    private async void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _connectionService.DisconnectAsync();
            LogMessage("切断しました");
        }
        catch (Exception ex)
        {
            LogMessage($"エラー: {ex.Message}");
        }
    }

    private void Connection_OnDataReceived(object? sender, ConnectionEventArgs e)
    {
        if (e.Data == null) return;
        LogMessage($"データ受信: {BitConverter.ToString(e.Data)}");
    }

    private void Connection_OnDataSent(object? sender, ConnectionEventArgs e)
    {
        if (e.Data == null) return;
        LogMessage($"データ送信: {BitConverter.ToString(e.Data)}");
    }

    private void Connection_OnError(object? sender, ConnectionEventArgs e)
    {
        LogMessage($"エラー: {e.Message}");
    }

    private void Connection_OnConnectionStateChanged(object? sender, bool connected)
    {
        Dispatcher.BeginInvoke(() =>
        {
            ConnectButton.IsEnabled = !connected;
            DisconnectButton.IsEnabled = connected;

            if (connected)
            {
                _statisticsTimer.Start();
            }
            else
            {
                _statisticsTimer.Stop();
                ClearStatistics();
            }
        });
    }

    private void StatisticsTimer_Tick(object? sender, EventArgs e)
    {
        if (_connectionService.Statistics != null)
        {
            UpdateStatistics(_connectionService.Statistics);
        }
    }

    private void UpdateStatistics(IConnectionStatistics stats)
    {
        BytesSentText.Text = FormatByteCount(stats.BytesSent);
        BytesReceivedText.Text = FormatByteCount(stats.BytesReceived);
        ErrorCountText.Text = stats.ErrorCount.ToString();
        UptimeText.Text = FormatTimeSpan(stats.Uptime);
        LastConnectedText.Text = FormatDateTime(stats.LastConnected);
        LastErrorText.Text = FormatDateTime(stats.LastError);
    }

    private void ClearStatistics()
    {
        BytesSentText.Text = string.Empty;
        BytesReceivedText.Text = string.Empty;
        ErrorCountText.Text = string.Empty;
        UptimeText.Text = string.Empty;
        LastConnectedText.Text = string.Empty;
        LastErrorText.Text = string.Empty;
    }

    private static string FormatByteCount(long bytes)
    {
        string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
        int suffixIndex = 0;
        double size = bytes;

        while (size >= 1024 && suffixIndex < suffixes.Length - 1)
        {
            size /= 1024;
            suffixIndex++;
        }

        return $"{size:N2} {suffixes[suffixIndex]}";
    }

    private static string FormatTimeSpan(TimeSpan span)
    {
        if (span.TotalDays >= 1)
        {
            return $"{(int)span.TotalDays}日 {span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
        }
        return $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}";
    }

    private static string FormatDateTime(DateTime dt)
    {
        return dt == DateTime.MinValue ? "-" : dt.ToString("yyyy/MM/dd HH:mm:ss");
    }

    public void LogMessage(string message)
    {
        Dispatcher.BeginInvoke(() =>
        {
            LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            LogTextBox.ScrollToEnd();
        });
    }

    private void ClearLogButton_Click(object sender, RoutedEventArgs e)
    {
        LogTextBox.Clear();
    }

        private void SaveLogButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "テキストファイル (*.txt)|*.txt|すべてのファイル (*.*)|*.*",
                DefaultExt = ".txt",
                FileName = $"connection_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    System.IO.File.WriteAllText(saveFileDialog.FileName, LogTextBox.Text);
                    LogMessage("ログを保存しました: " + saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"ログの保存中にエラーが発生しました: {ex.Message}", "エラー", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
