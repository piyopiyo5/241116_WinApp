using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WpfApp1.Services;

namespace WpfApp1.Controls;

public partial class CustomWindow : UserControl, INotifyPropertyChanged
{
    private readonly ConnectionService _connectionService;
    private bool _isConnected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsConnected
    {
        get => _isConnected;
        private set
        {
            if (_isConnected != value)
            {
                _isConnected = value;
                OnPropertyChanged();
            }
        }
    }

    public CustomWindow()
    {
        InitializeComponent();
        _connectionService = ConnectionService.Instance;
        _connectionService.OnConnectionStateChanged += (s, connected) => IsConnected = connected;
        DataContext = this;
    }

    private async void SendButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(SendDataTextBox.Text))
            return;

        try
        {
            var data = Encoding.UTF8.GetBytes(SendDataTextBox.Text);
            await _connectionService.SendAsync(data);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"送信エラー: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
