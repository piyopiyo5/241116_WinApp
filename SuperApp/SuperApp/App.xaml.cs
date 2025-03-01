using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace SuperApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // メインウィンドウを表示
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // アプリ終了時の処理
            base.OnExit(e);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // 例外処理（未処理の例外をキャッチ）
            MessageBox.Show($"エラーが発生しました: {e.Exception.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true; // アプリを強制終了しないようにする
        }
    }

}
