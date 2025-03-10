using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp1.Connections.Interfaces;
using WpfApp1.Connections.Implementations;
using WpfApp1.Connections.Logging;
using WpfApp1.Connections.Models;

namespace WpfApp1;

public partial class MainWindow : Window
{
    private IConnection? _connection;
    private readonly FileLogger _logger;

    public MainWindow()
    {
        InitializeComponent();
        _logger = new FileLogger("connection.log");

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
        NetworkInterfaceComboBox.Visibility = tcpUdpVisibility;
        AddressTextBox.Visibility = tcpUdpVisibility;
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
                _connection = new SerialConnection(settings, _logger);
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

                _connection = CommunicationTypeComboBox.SelectedIndex == 0
                    ? new TcpConnection(settings, _logger)
                    : new UdpConnection(settings, _logger) as IConnection;
            }

            _connection.OnDataReceived += Connection_OnDataReceived;
            _connection.OnError += Connection_OnError;

            if (await _connection.Connect())
            {
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
                SendTestButton.IsEnabled = true;
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

    private void DisconnectButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            _connection?.Disconnect();
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
            SendTestButton.IsEnabled = false;
            LogMessage("切断しました");
        }
        catch (Exception ex)
        {
            LogMessage($"エラー: {ex.Message}");
        }
    }

    private async void SendTestButton_Click(object sender, RoutedEventArgs e)
    {
        if (_connection == null) return;

        try
        {
            var testData = new byte[] { 0x01, 0x02, 0x03, 0x04 };
            if (await _connection.SendAsync(testData))
            {
                LogMessage("テストデータを送信しました");
            }
            else
            {
                LogMessage("送信に失敗しました");
            }
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

    private void Connection_OnError(object? sender, ConnectionEventArgs e)
    {
        LogMessage($"エラー: {e.Message}");
    }

    private void LogMessage(string message)
    {
        Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
        {
            LogTextBox.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
            LogTextBox.ScrollToEnd();
        }));
    }

    protected override void OnClosed(EventArgs e)
    {
        _connection?.Disconnect();
        base.OnClosed(e);
    }
}
