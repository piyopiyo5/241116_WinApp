// --------------------------------------------------
// MainViewModel.cs
// --------------------------------------------------

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

            TaskTimeData = new ObservableCollection<DisplayRow>();
            LoadTaskTimeData();

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }



        #region セッション管理のコード
        private static bool _isLocked = false;
        private bool IsLocked { get { return _isLocked; } }

        // セッション変更時のイベントハンドラ
        static void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {

            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                    //Console.WriteLine(DateTime.Now + "：画面ロック");
                    _isLocked = true;
                    break;

                case SessionSwitchReason.SessionUnlock:
                    //Console.WriteLine(DateTime.Now + "：画面ロック解除");
                    _isLocked = false;
                    break;
            }
        }
        #endregion

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
            if (!IsLocked)
            {
                UpdateClock();
                foreach (var timer in CountUpTimers)
                {
                    timer.UpdateCountUpTimer();
                }

                UpdateTotalCountUpTimer();

                EnableNumLock();
            }

            if (IsActiveWindowLogging)
            {
                ActiveWindowTitle = GetActiveWindowTitle();
            }
        }

        // 時計表示を更新する
        private void UpdateClock()
        {
            ClockText = DateTime.Now.ToString("HH:mm:ss");
        }

        // 合計時間を更新する
        private void UpdateTotalCountUpTimer()
        {
            _totalEelapsedTime = TimeSpan.Zero;
            foreach (var timer in CountUpTimers)
            {
                _totalEelapsedTime += timer.ElapsedTime;
            }
            TotalCountUpTimerText = _totalEelapsedTime.ToString(@"hh\:mm\:ss");
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


        public ObservableCollection<DisplayRow> TaskTimeData { get; set; }
        public class DisplayRow
        {
            public string Date { get; set; }
            public string TaskName { get; set; }
            public string TimeSpent { get; set; }
        }

        private void LoadTaskTimeData()
        {
            try
            {
                string jsonFilePath = "data.json"; // JSONファイルのパス
                if (!File.Exists(jsonFilePath))
                {
                    // ファイルがない場合は空のリストを表示
                    return;
                }

                string jsonData = File.ReadAllText(jsonFilePath);

                // JSONをデシリアライズ
                var dailyData = JsonSerializer.Deserialize<Dictionary<string, AppState>>(jsonData);

                if (dailyData == null) return;

                // データを変換してTaskTimeDataに追加
                foreach (var dateEntry in dailyData)
                {
                    string date = dateEntry.Key;
                    foreach (var timer in dateEntry.Value.Timers)
                    {
                        TaskTimeData.Add(new DisplayRow
                        {
                            Date = date,
                            TaskName = timer.CountUpTimerName,
                            TimeSpent = timer.CountUpTimerText
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // 必要に応じてエラーログを追加
            }
        }

        #region 設定のコード
        // -----------------------------------------------------------------------------------------------------------------------
        private bool _isAlwaysOnTop = true;
        private bool _isNumLockKeep = true;

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

        // NumlockKeepの有効化
        private DelegateCommand? _enableNumlockCommand;
        public DelegateCommand EnableNumlockCommand
        {
            get
            {
                return _enableNumlockCommand ??= new DelegateCommand(
                    parameter =>
                    {
                        _isNumLockKeep = true;
                        UpdateNumlockCommand();
                    },
                    _ => !_isNumLockKeep
                    );

            }
        }

        // NumlockKeepの無効化
        private DelegateCommand? _disableNumlockCommand;
        public DelegateCommand DisableNumlockCommand
        {
            get
            {
                return _disableNumlockCommand ??= new DelegateCommand(
                    parameter =>
                    {
                        _isNumLockKeep = false;
                        UpdateNumlockCommand();
                    },
                    _ => _isNumLockKeep
                    );
            }
        }

        // NumLock有効化
        private void EnableNumLock()
        {
            if (_isNumLockKeep)
            {
                bool isNumLockOn = Keyboard.IsKeyToggled(Key.NumLock);

                if (!isNumLockOn)
                {
                    ToggleNumLock();
                }
            }
        }

        private void ToggleNumLock()
        {
            // NumLockキーをシミュレート
            keybd_event(VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | 0, 0);
            keybd_event(VK_NUMLOCK, 0x45, KEYEVENTF_EXTENDEDKEY | KEYEVENTF_KEYUP, 0);
        }

        // Win32 APIのインポート
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        private const byte VK_NUMLOCK = 0x90; // NumLockキーの仮想キーコード
        private const uint KEYEVENTF_EXTENDEDKEY = 0x0001; // 拡張キー
        private const uint KEYEVENTF_KEYUP = 0x0002; // キーを離す

        // コマンドの状態を更新する
        private void UpdateAlwaysOnTopCommand()
        {
            EnableAlwaysOnTopCommand.RaiseCanExecuteChanged();
            DisableAlwaysOnTopCommand.RaiseCanExecuteChanged();
        }

        private void UpdateNumlockCommand()
        {
            EnableNumlockCommand.RaiseCanExecuteChanged();
            DisableNumlockCommand.RaiseCanExecuteChanged();
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
                        CountUpTimers.Add(new CountUpTimer("Timer" + Convert.ToString(CountUpTimers.Count)));
                        UpdateOtherTimers();
                    },
                    _ => true);
            }
        }

        // カウントアップタイマーを削除
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
                            UpdateTotalCountUpTimer();
                        }
                    },
                    parameter => parameter is CountUpTimer // 引数がCountUpTimerの場合にのみ実行可能
                );
            }
        }

        // カウントアップタイマーを停止
        private DelegateCommand? _stopAllTimerCommand;
        public DelegateCommand StopAllTimerCommand
        {
            get
            {
                return _stopAllTimerCommand ??= new DelegateCommand(
                    _ =>
                    {
                        // タイマーを停止
                        foreach (var timer in CountUpTimers)
                        {
                            timer.StopTimer();
                        }
                    },
                    _ => true
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
        // アプリデータを保存
        public void SaveAppData()
        {
            try
            {
                // 現在の日付をキーとして保存する
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");

                // 経過時間がゼロのタイマーは削除する
                RemoveZeroElapsedTimers(CountUpTimers);

                // タイマーのデータをリストとして取得
                var timersData = CountUpTimers.Select(t => new
                {
                    t.CountUpTimerName,
                    t.CountUpTimerText,
                    t.IsFavorite
                }).ToList();

                // 保存用の階層構造データ
                var appState = new Dictionary<string, object>();

                // 過去のデータを読み込む
                if (System.IO.File.Exists("data.json"))
                {
                    string existingJson = System.IO.File.ReadAllText("data.json");
                    var existingState = JsonSerializer.Deserialize<Dictionary<string, object>>(existingJson);

                    if (existingState != null)
                    {
                        foreach (var entry in existingState)
                        {
                            appState[entry.Key] = entry.Value;
                        }
                    }
                }

                // 現在の日付のデータを追加または更新
                appState[currentDate] = new
                {
                    TimerCount = CountUpTimers.Count, // タイマー数を保存
                    Timers = timersData
                };

                // JSON 形式で保存
                var json = JsonSerializer.Serialize(appState, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 非ASCII文字をエスケープしない
                });

                // 保存ファイルパスを指定
                System.IO.File.WriteAllText("data.json", json);
            }
            catch (Exception ex)
            {
                // エラー時のログや通知を実行（デバッグ出力として記録）
                System.Diagnostics.Debug.WriteLine($"アプリ状態の保存中にエラーが発生しました: {ex.Message}");
            }
        }

        // 経過時間がゼロのタイマーを削除する
        void RemoveZeroElapsedTimers(ObservableCollection<CountUpTimer> countUpTimers)
        {
            for (int i = countUpTimers.Count - 1; i >= 0; i--)
            {
                if (countUpTimers[i].ElapsedTime == TimeSpan.Zero && countUpTimers[i].IsFavorite == false)
                {
                    countUpTimers.RemoveAt(i);
                }
            }
        }

        // アプリデータを読み込み
        public void LoadAppState()
        {
            try
            {
                // 保存されたアプリ状態のファイルが存在するか確認
                if (System.IO.File.Exists("data.json"))
                {
                    var json = System.IO.File.ReadAllText("data.json");

                    // JSON データを日付ごとの辞書として読み込む
                    var appState = JsonSerializer.Deserialize<Dictionary<string, object>>(json);

                    if (appState != null && appState.Any())
                    {
                        // 最も新しい日付を取得
                        var latestDate = appState.Keys.Max();

                        if (latestDate == null)
                        {
                            return;
                        }

                        // 最新データを取得
                        var latestDataJson = appState[latestDate]?.ToString();
                        if (!string.IsNullOrEmpty(latestDataJson))
                        {
                            var latestData = JsonSerializer.Deserialize<LatestAppState>(latestDataJson);
                            if (latestData != null)
                            {
                                // タイマーを復元
                                foreach (var timerData in latestData.Timers)
                                {
                                    var newTimer = new CountUpTimer(timerData.CountUpTimerName, timerData.IsFavorite);
                                    // 日付が今日なら経過時間も復元
                                    if (latestDate == DateTime.Now.ToString("yyyy-MM-dd"))
                                    {
                                        newTimer.ElapsedTime = string.IsNullOrEmpty(timerData.CountUpTimerText) ? TimeSpan.Zero : TimeSpan.Parse(timerData.CountUpTimerText);
                                        newTimer.UpdateCountUpTimer();
                                    }
                                    CountUpTimers.Add(newTimer);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // ファイルがない場合、タイマーを一つ追加
                    CountUpTimers.Add(new CountUpTimer("タイマー0"));
                }

                // 各タイマーに他のタイマーのリストを設定する
                UpdateOtherTimers();

                // 先頭のタイマーをスタートする
                if (CountUpTimers.Count > 0)
                {
                    CountUpTimers[0].StartTimer();
                }
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
            public List<TimerData>? Timers { get; set; }
            public int TimerCount { get; set; }
        }

        public class TimerData
        {
            public string? CountUpTimerName { get; set; }
            public string? CountUpTimerText { get; set; }
            public bool IsFavorite { get; set; }
        }

        public class LatestAppState
        {
            public int TimerCount { get; set; }
            public List<TimerData> Timers { get; set; } = new();
        }

        // アプリ設定を保存
        public void SaveAppSettings()
        {
            try
            {
                // 保存用の階層構造データ
                var appSettings = new Dictionary<string, object>
                {
                    { "AlwaysOnTop", _isAlwaysOnTop },
                    { "NumLockKeep", _isNumLockKeep }
                };

                // JSON 形式で保存
                var json = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping // 非ASCII文字をエスケープしない
                });

                // 保存ファイルパスを指定
                System.IO.File.WriteAllText("settings.json", json);
            }
            catch (Exception ex)
            {
                // エラー時のログや通知を実行（デバッグ出力として記録）
                System.Diagnostics.Debug.WriteLine($"アプリ設定の保存中にエラーが発生しました: {ex.Message}");
            }
        }

        // アプリ設定を読み込み
        public void LoadAppSettings()
        {
            try
            {
                // 設定ファイルが存在するか確認
                if (File.Exists("settings.json"))
                {
                    var json = File.ReadAllText("settings.json");

                    // JSON データをオブジェクトに変換
                    var appSettings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

                    if (appSettings != null)
                    {
                        // AlwaysOnTopの設定を読み込む
                        if (appSettings.TryGetValue("AlwaysOnTop", out var alwaysOnTopElement) &&
                            alwaysOnTopElement.ValueKind == JsonValueKind.True || alwaysOnTopElement.ValueKind == JsonValueKind.False)
                        {
                            _isAlwaysOnTop = alwaysOnTopElement.GetBoolean();
                            _window.Topmost = _isAlwaysOnTop;
                        }

                        // NumLockKeepの設定を読み込む
                        if (appSettings.TryGetValue("NumLockKeep", out var numLockKeepElement) &&
                            numLockKeepElement.ValueKind == JsonValueKind.True || numLockKeepElement.ValueKind == JsonValueKind.False)
                        {
                            _isNumLockKeep = numLockKeepElement.GetBoolean();
                        }

                        UpdateNumlockCommand();
                    }
                }
            }
            catch (JsonException jsonEx)
            {
                System.Diagnostics.Debug.WriteLine($"JSON解析エラー: {jsonEx.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"アプリ設定の読み込み中にエラーが発生しました: {ex.Message}");
            }
        }
        #endregion

        #region 業務解析のコード
        // -----------------------------------------------------------------------------------------------------------------------
        private bool _isActiveWindowLogging = true;
        private bool IsActiveWindowLogging { get { return _isActiveWindowLogging; } }

        // ActiveWindowLoggingの無効化
        private DelegateCommand? _disableActiveWindowLoggingCommand;
        public DelegateCommand DisableActiveWindowLoggingCommand
        {
            get
            {
                return _disableActiveWindowLoggingCommand ??= new DelegateCommand(
                    _ =>
                    {
                        _isActiveWindowLogging = false;
                        UpdateActiveWindowLoggingCommand();
                    },
                    _ => _isActiveWindowLogging
                    );
            }
        }

        // ActiveWindowLoggingの有効化
        private DelegateCommand? _enableActiveWindowLoggingCommand;
        public DelegateCommand EnableActiveWindowLoggingCommand
        {
            get
            {
                return _enableActiveWindowLoggingCommand ??= new DelegateCommand(
                    parameter =>
                    {
                        _isActiveWindowLogging = true;
                        UpdateActiveWindowLoggingCommand();
                    },
                    _ => !_isActiveWindowLogging
                    );

            }
        }

        private void UpdateActiveWindowLoggingCommand()
        {
            EnableActiveWindowLoggingCommand.RaiseCanExecuteChanged();
            DisableActiveWindowLoggingCommand.RaiseCanExecuteChanged();
        }

        // アクティブウィンドウタイトル
        private string _activeWindowTitle = " ";
        public string ActiveWindowTitle
        {
            get { return _activeWindowTitle; }
            private set { SetProperty(ref _activeWindowTitle, value); }
        }

        // 外部のWindows APIを宣言
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        // アクティブウィンドウのタイトルを取得する関数
        public static string GetActiveWindowTitle()
        {
            // アクティブなウィンドウのハンドルを取得
            IntPtr hWnd = GetForegroundWindow();

            // アクティブウィンドウのタイトルを格納するStringBuilder
            StringBuilder windowTitle = new StringBuilder(256);

            // ウィンドウのタイトルを取得
            GetWindowText(hWnd, windowTitle, windowTitle.Capacity);

            // 取得したタイトルを返す
            return windowTitle.ToString();
        }

        #endregion
    }
}