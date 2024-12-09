// ------------------------------------------------------------------
// App.xaml.cs
// ------------------------------------------------------------------

using System.Configuration;
using System.Data;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows;
using WpfApp1.ViewModels;
using WpfApp1.Views;
using static WpfApp1.App;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        MainViewModel? _mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);



            var w = new MainView();
            _mainViewModel = new MainViewModel(w);
            w.DataContext = _mainViewModel;

            // アプリ状態をロード
            _mainViewModel.LoadAppState();
            _mainViewModel.LoadAppSettings();

            w.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // アプリ終了時に保存処理を実行
            _mainViewModel?.SaveAppData();
            _mainViewModel?.SaveAppSettings();
        }
    }
}
