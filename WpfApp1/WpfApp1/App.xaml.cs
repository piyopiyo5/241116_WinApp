using System.Configuration;
using System.Data;
using System.Windows;
using WpfApp1.ViewModels;
using WpfApp1.Views;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var w = new MainView();
            Application.Current.MainWindow = w;
            var vm = new MainViewModel();
            w.DataContext = vm;
            w.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (Application.Current.MainWindow.DataContext is MainViewModel viewModel)
            {
                viewModel.SaveTimers();
            }
            base.OnExit(e);
        }

    }
}
