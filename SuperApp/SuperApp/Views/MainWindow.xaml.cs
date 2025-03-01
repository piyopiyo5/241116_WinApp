using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SuperApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        
        public MainWindow()
        {
            InitializeComponent();
            StartClock();
        }

        #region MenuBar
        private void OnVersionInfoMenuClick(object sender, RoutedEventArgs e)
        {
            SuperApp.Views.InfoView versionInformationWindow = new SuperApp.Views.InfoView("Version: 1.0.0");
            versionInformationWindow.ShowDialog();
        }
        #endregion MenuBar

        #region StatusBar
        private DispatcherTimer? _timerForClock;
        private void StartClock()
        {
            _timerForClock = new DispatcherTimer();
            _timerForClock.Interval = TimeSpan.FromSeconds(1);
            _timerForClock.Tick += UpdateClock;
            _timerForClock.Start();
        }

        private void UpdateClock(object? sender, EventArgs e)
        {
            ClockText.Text = DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion StatusBar
    }
}