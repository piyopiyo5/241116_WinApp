// --------------------------------------------------
// MainViewModel.cs
// --------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// メイン画面のViewModel
    /// </summary>
    internal class MainViewModel : NotificationObject
    {
        private Window _window;

        public MainViewModel(Window window)
        {
            // ウィンドウを保持する
            _window = window;
            _window.Topmost = true;

            // 1秒おきにTickイベントを発生させるタイマーを生成する
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(Const.TimerTickInterval)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();
            
            // カウントアップタイマーのリストを生成する
            CountUpTimers = new ObservableCollection<CountUpTimer>();
        }

        #region 時計表示のコード
        // -----------------------------------------------------------------------------------------------------------------------
        // タイマー
        private DispatcherTimer _timer;
        private string _clockText = string.Empty;
        public string ClockText
        {
            get { return _clockText; }
            private set { SetProperty(ref _clockText, value); }
        }

        // １秒ごとに呼び出されるイベントハンドラ
        private void Timer_Tick(object? sender, EventArgs e)
        {
            UpdateClock();
            foreach (var timer in CountUpTimers)
            {
                timer.UpdateCountUpTimer();
            }
            if (IsCountUpTimerRunning())
            {
                _totalEelapsedTime += TimeSpan.FromSeconds(1);
                TotalCountUpTimerText = _totalEelapsedTime.ToString(@"hh\:mm\:ss");
            }
        }

        // 時計表示を更新する
        private void UpdateClock()
        {
            ClockText = DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion

        #region カウントアップタイマーのコード

        // 複数のタイマーを管理するObservableCollection
        public ObservableCollection<CountUpTimer> CountUpTimers { get; }

        // タイマーの合計時間
        private TimeSpan _totalEelapsedTime = TimeSpan.Zero; // 経過時間
        private string _totalCountUpTimerText = "00:00:00";
        public string TotalCountUpTimerText
        {
            get { return "Total: " + _totalCountUpTimerText; }
            private set { SetProperty(ref _totalCountUpTimerText, value); }
        }

        #endregion

        #region 設定のコード
        // -----------------------------------------------------------------------------------------------------------------------
        private bool _isAlwaysOnTop = true;

        // AlwaysOnTopの有効化
        private DelegateCommand? _enableAlwaysOnTop;
        public DelegateCommand EnableAlwaysOnTopCommand
        {
            get
            {
                return _enableAlwaysOnTop ??= new DelegateCommand(
                    _ =>
                    {
                        _window.Topmost = true;
                        _isAlwaysOnTop = true;
                        UpdateAlwaysOnTopCommand();
                    },
                    _ => !_isAlwaysOnTop
                    );
            }
        }

        // AlwaysOnTopの無効化
        private DelegateCommand? _disableAlwaysOnTop;
        public DelegateCommand DisableAlwaysOnTopCommand
        {
            get
            {
                return _disableAlwaysOnTop ??= new DelegateCommand(
                    _ =>
                    {
                        _window.Topmost = false;
                        _isAlwaysOnTop = false;
                        UpdateAlwaysOnTopCommand();
                    },
                    _ => _isAlwaysOnTop
                    );
            }
        }

        // コマンドの状態を更新する
        private void UpdateAlwaysOnTopCommand()
        {
            EnableAlwaysOnTopCommand.RaiseCanExecuteChanged();
            DisableAlwaysOnTopCommand.RaiseCanExecuteChanged();
        }

        // カウントアップタイマーを追加
        private DelegateCommand? _addCountUpTimerCommand;
        public DelegateCommand AddCountUpTimerCommand
        {
            get
            {
                return _addCountUpTimerCommand ??= new DelegateCommand(
                    _ =>
                    {
                        // カウントアップタイマーの追加
                        CountUpTimers.Add(new CountUpTimer("タイマー" + Convert.ToString(CountUpTimers.Count)));
                        UpdateOtherTimers();
                    },
                    _ => true);
            }
        }

        private DelegateCommand? _removeTimerCommand;
        public DelegateCommand RemoveTimerCommand
        {
            get
            {
                return _removeTimerCommand ??= new DelegateCommand(
                    parameter =>
                    {
                        // 引数として渡されたタイマーを削除
                        var timerToRemove = parameter as CountUpTimer;
                        if (timerToRemove != null)
                        {
                            CountUpTimers.Remove(timerToRemove);
                            UpdateOtherTimers();
                        }
                    },
                    parameter => parameter is CountUpTimer // 引数がCountUpTimerの場合にのみ実行可能
                );
            }
        }

        // OtherTimersを更新する
        private void UpdateOtherTimers()
        {
            foreach (var timer in CountUpTimers)
            {
                timer.OtherTimers = CountUpTimers.Where(t => t != timer).ToList();
            }
        }

        // 動作中のカウントアップタイマーがあるかどうか
        private bool IsCountUpTimerRunning()
        {
            return CountUpTimers.Any(timer => timer._isCountUpTimerRunning);
        }

        #endregion

        #region 保存と読み込みのコード
        // -----------------------------------------------------------------------------------------------------------------------
        // アプリ状態を保存
        public void SaveAppState()
        {
            try
            {
                var timersData = CountUpTimers.Select(t => new
                {
                    t.CountUpTimerName,
                    t.CountUpTimerText
                }).ToList();

                var appState = new
                {
                    Timers = timersData,
                    TimerCount = CountUpTimers.Count // タイマー数を保存
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

        // アプリ状態を読み込み
        public void LoadAppState()
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
                        // タイマーを復元
                        for (int i = 0; i < appState.TimerCount; i++)
                        {
                            // 保存されたタイマー情報を基にタイマーを復元
                            var timerData = appState.Timers[i];
                            if (timerData != null)
                            {
                                var newTimer = new CountUpTimer(timerData.CountUpTimerName);
                                CountUpTimers.Add(newTimer);
                            }
                        }
                    }
                }
                else
                {
                    // タイマーを一つ追加
                    CountUpTimers.Add(new CountUpTimer("タイマー0"));
                }

                // 各タイマーに他のタイマーのリストを設定する
                UpdateOtherTimers();

                // 先頭のタイマーをスタートする
                CountUpTimers[0].StartTimer();
            }
            catch (Exception ex)
            {
                // エラー時のログや通知を実行（デバッグ出力として記録）
                System.Diagnostics.Debug.WriteLine($"アプリ状態の読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        // 状態保持用のクラス
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
        #endregion
    }
}