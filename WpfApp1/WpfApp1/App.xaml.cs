using System.Collections.Specialized;
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
            var savedTimerNames = LoadTimerNames(); // タイマー名の読み込み
            var w = new MainView();
            var vm = new MainViewModel(w);
            w.DataContext = vm;
            w.Show();
        }

        //private StringCollection LoadTimerNames()
        //{
        //    return Properties.Values.
        //}

        //private void SaveTimerNames(StringCollection timerNames)
        //{
        //    Properties.Settings.Default.TimerNames = timerNames;
        //    Properties.Settings.Default.Save();
        //}
    }
}
