using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp1.Connections.Implementations;
using WpfApp1.Connections.Interfaces;
using WpfApp1.Connections.Logging;
using WpfApp1.Connections.Models;
using WpfApp1.Services;

namespace WpfApp1.Controls;

public partial class CommonWindow : UserControl
{
    private readonly ConnectionService _connectionService;
    private readonly ILogger _logger;

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
        });
    }

    public void LogMessage(string message)
    {
        Dispatcher.BeginInvoke(() =>
        {
            LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            LogTextBox.ScrollToEnd();
        });
    }
}
