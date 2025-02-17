// --------------------------------------------------
// MainViewModel.cs
// --------------------------------------------------

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
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
                string jsonFilePath = "data.json";
                if (!File.Exists(jsonFilePath))
                {
                    return;
                }

                string jsonData = File.ReadAllText(jsonFilePath);
                var appData = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonData);

                if (appData != null && appData.ContainsKey("Header") && appData.ContainsKey("Body"))
                {
                    var header = JsonSerializer.Deserialize<Dictionary<string, string>>(appData["Header"].ToString());
                    if (header != null && header.ContainsKey("Version") && header["Version"] == "1.0.0")
                    {
                        var body = JsonSerializer.Deserialize<Dictionary<string, object>>(appData["Body"].ToString());
                        if (body != null)
                        {
                            foreach (var dateEntry in body)
                            {
                                string date = dateEntry.Key;
                                var dayDataJson = dateEntry.Value?.ToString();
                                if (!string.IsNullOrEmpty(dayDataJson))
                                {
                                    var dayData = JsonSerializer.Deserialize<AppData>(dayDataJson);
                                    if (dayData != null)
                                    {
                                        foreach (var timer in dayData.Timers)
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
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"タスク時間データの読み込み中にエラーが発生しました: {ex.Message}");
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

        private const string DatabaseFile = "db.db";

        // アプリデータを保存
        public void SaveAppData()
        {

            try
            {
                string currentDate = DateTime.Now.ToString("yyyy-MM-dd");
                RemoveZeroElapsedTimers(CountUpTimers);

                EnsureDatabaseExists();

                using (var connection = new SQLiteConnection($"Data Source={DatabaseFile};Version=3;"))
                {
                    connection.Open();

                    string insertQuery = @"
                    INSERT OR REPLACE INTO CountUpTimers 
                    (Date, CountUpTimerName, CountUpTimerText, IsFavorite) 
                    VALUES (@Date, @CountUpTimerName, @CountUpTimerText, @IsFavorite);
                ";

                    using (var command = new SQLiteCommand(insertQuery, connection))
                    {
                        foreach (var timer in CountUpTimers)
                        {
                            command.Parameters.Clear();
                            command.Parameters.AddWithValue("@Date", currentDate);
                            command.Parameters.AddWithValue("@CountUpTimerName", timer.CountUpTimerName);
                            command.Parameters.AddWithValue("@CountUpTimerText", timer.CountUpTimerText);
                            command.Parameters.AddWithValue("@IsFavorite", timer.IsFavorite ? 1 : 0);

                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"アプリ状態の保存中にエラーが発生しました: {ex.Message}");
            }
        }

        private void EnsureDatabaseExists()
        {
            if (!File.Exists(DatabaseFile))
            {
                SQLiteConnection.CreateFile(DatabaseFile);
            }

            using (var connection = new SQLiteConnection($"Data Source={DatabaseFile};Version=3;"))
            {
                connection.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS CountUpTimers (
                    Date TEXT NOT NULL,
                    CountUpTimerName TEXT NOT NULL,
                    CountUpTimerText TEXT,
                    IsFavorite INTEGER,
                    PRIMARY KEY (Date, CountUpTimerName)
                );
            ";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
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
        public void LoadAppData()
        {
            try
            {
                if (!File.Exists(DatabaseFile))
                {
                    CountUpTimers.Add(new CountUpTimer("タイマー0"));
                    return;
                }

                using (var connection = new SQLiteConnection($"Data Source={DatabaseFile};Version=3;"))
                {
                    connection.Open();

                    // 最新の日付を取得
                    string getLatestDateQuery = "SELECT DISTINCT Date FROM CountUpTimers ORDER BY Date DESC LIMIT 1;";

                    string latestDate = null;
                    using (var command = new SQLiteCommand(getLatestDateQuery, connection))
                    {
                        var result = command.ExecuteScalar();
                        if (result != null)
                        {
                            latestDate = result.ToString();
                        }
                    }

                    if (string.IsNullOrEmpty(latestDate))
                    {
                        CountUpTimers.Add(new CountUpTimer("タイマー0"));
                        return;
                    }

                    // 最新の日付のデータを取得
                    string getTimersQuery = @"
                    SELECT CountUpTimerName, CountUpTimerText, IsFavorite 
                    FROM CountUpTimers 
                    WHERE Date = @Date;
                ";

                    using (var command = new SQLiteCommand(getTimersQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Date", latestDate);

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string name = reader["CountUpTimerName"].ToString();
                                string text = reader["CountUpTimerText"].ToString();
                                bool isFavorite = reader.GetInt32(reader.GetOrdinal("IsFavorite")) == 1;

                                var newTimer = new CountUpTimer(name, isFavorite);

                                if (latestDate == DateTime.Now.ToString("yyyy-MM-dd"))
                                {
                                    newTimer.ElapsedTime = string.IsNullOrEmpty(text) ? TimeSpan.Zero : TimeSpan.Parse(text);
                                    newTimer.UpdateCountUpTimer();
                                }

                                CountUpTimers.Add(newTimer);
                            }
                        }
                    }
                }

                UpdateOtherTimers();

                if (CountUpTimers.Count > 0)
                {
                    CountUpTimers[0].StartTimer();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"アプリ状態の読み込み中にエラーが発生しました: {ex.Message}");
            }
        }

        // 状態保持用のクラス
        public class AppData
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

        public class LatestAppData
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