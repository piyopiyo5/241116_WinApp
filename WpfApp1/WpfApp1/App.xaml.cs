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
        MainViewModel _mainViewModel;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);



            var w = new MainView();
            _mainViewModel = new MainViewModel(w);
            w.DataContext = _mainViewModel;

            // アプリ状態をロード
            LoadAppState();

            w.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            // アプリ終了時に保存処理を実行
            SaveAppState();
        }

        private void SaveAppState()
        {
            try
            {
                var timersData = _mainViewModel.CountUpTimers.Select(t => new
                {
                    t.CountUpTimerName,
                    t.CountUpTimerText
                }).ToList();

                var appState = new
                {
                    Timers = timersData,
                    TimerCount = _mainViewModel.CountUpTimers.Count // タイマー数を保存
                };

                var json = JsonSerializer.Serialize(appState, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 非ASCII文字をエスケープしない
                });

                // 保存ファイルパスを指定
                System.IO.File.WriteAllText("appState.json", json);
            }
            catch (Exception ex)
            {
                // エラー時のログや通知を実行（デバッグ出力として記録）
                System.Diagnostics.Debug.WriteLine($"アプリ状態の保存中にエラーが発生しました: {ex.Message}");
            }
        }

        private void LoadAppState()
        {
            try
            {
                // 保存されたアプリ状態のファイルが存在するか確認
                if (System.IO.File.Exists("appState.json"))
                {
                    var json = System.IO.File.ReadAllText("appState.json");
                    var appState = JsonSerializer.Deserialize<AppState>(json);

                    if (appState != null)
                    {
                        // タイマー数と名前を復元
                        _mainViewModel.CountUpTimers.Clear();

                        for (int i = 0; i < appState.TimerCount; i++)
                        {
                            //var timerData = appState.Timers.FirstOrDefault(t => t.CountUpTimerName == "タイマー" + i.ToString());
                            var timerData = appState.Timers[i];
                            if (timerData != null)
                            {
                                var newTimer = new CountUpTimer(timerData.CountUpTimerName)
                                {
                                    CountUpTimerText = timerData.CountUpTimerText
                                };
                                _mainViewModel.CountUpTimers.Add(newTimer);
                            }
                        }

                        //foreach (var timerData in timersData)
                        //{
                        //    var newTimer = new CountUpTimer(timerData.CountUpTimerName)
                        //    {
                        //        CountUpTimerText = timerData.CountUpTimerText
                        //    };
                        //    CountUpTimers.Add(newTimer);
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                // エラー時のログや通知を実行（デバッグ出力として記録）
                System.Diagnostics.Debug.WriteLine($"アプリ状態の読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        public class AppState
        {
            public List<TimerData> Timers { get; set; }
            public int TimerCount { get; set; }
        }

        public class TimerData
        {
            public string CountUpTimerName { get; set; }
            public string CountUpTimerText { get; set; }
        }


    }
}
