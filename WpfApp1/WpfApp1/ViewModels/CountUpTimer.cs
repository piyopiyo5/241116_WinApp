// -----------------------------------------------------------------
// CountUpTimer.cs
// -----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WpfApp1.Views;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// カウントアップタイマー
    /// </summary>
    internal class CountUpTimer : NotificationObject
    {
        private TimeSpan _elapsedTime = TimeSpan.Zero; // 経過時間
        public TimeSpan ElapsedTime
        {
            get { return _elapsedTime; }
            set { SetProperty(ref _elapsedTime, value); }
        }

        public bool _isCountUpTimerRunning = false; // カウントアップタイマーが動作中かどうか

        public CountUpTimer(string? TimerName)
        {
            // 引数が null または空文字の場合にデフォルト名を設定
            _countUpTimerName = string.IsNullOrEmpty(TimerName) ? "タイマー" : TimerName;
        }

        // カウントアップタイマーの名前
        private string _countUpTimerName;
        public string CountUpTimerName
        {
            get { return _countUpTimerName; }
            set { SetProperty(ref _countUpTimerName, value); }
        }

        // カウントアップタイマーの表示文字列
        private string _countUpTimerText = "00:00:00";
        public string CountUpTimerText
        {
            get { return _countUpTimerText; }
            set { SetProperty(ref _countUpTimerText, value); }
        }

        // 他のタイマーの参照リスト
        public List<CountUpTimer> OtherTimers { get; set; } = new List<CountUpTimer>();

        // タイマースタートコマンド
        private DelegateCommand? _timerStartCommand;
        public DelegateCommand TimerStartCommand
        {
            get
            {
                return _timerStartCommand ??= new DelegateCommand(
                    _ =>
                    {
                        // 他のタイマーを停止
                        foreach (var timer in OtherTimers)
                        {
                            timer.StopTimer();
                        }

                        // 自身をスタート
                        StartTimer();
                    },
                    _ => !_isCountUpTimerRunning);
            }
        }

        // タイマーストップコマンド
        private DelegateCommand? _timerStopCommand;
        public DelegateCommand TimerStopCommand
        {
            get
            {
                return _timerStopCommand ??= new DelegateCommand(
                    _ =>
                    {
                        StopTimer();
                    },
                    _ => _isCountUpTimerRunning);
            }
        }

        // タイマー修正コマンド（修正ウィンドウオープン）
        private DelegateCommand? _timerEditCommand;
        public DelegateCommand TimerEditCommand
        {
            get
            {
                return _timerEditCommand ??= new DelegateCommand(
                    _ =>
                    {
                        // メインウィンドウの Topmost を一時的にfalseにする
                        Window mainWindow = Application.Current.MainWindow;
                        mainWindow.Topmost = false;

                        // モーダルウィンドウを開く
                        TimerEditView editWindow = new TimerEditView(this)
                        {
                            Owner = Window.GetWindow(mainWindow),
                            WindowStartupLocation = WindowStartupLocation.CenterOwner
                        }; ;
                        editWindow.ShowDialog();

                        // モーダルウィンドウを最前面に持ってくる
                        editWindow.Activate();

                        // メインウィンドウのTopmostをtrueに戻す
                        mainWindow.Topmost = true;
                    },
                    _ => !_isCountUpTimerRunning); // タイマー停止中に修正できる
            }
        }

        // タイマーインクリメントコマンド
        private DelegateCommand? _timerIncrementCommand;
        public DelegateCommand TimerIncrementCommand
        {
            get
            {
                return _timerIncrementCommand ??= new DelegateCommand(
                    _ =>
                    {
                        // タイマーを10分インクリメントする
                        _elapsedTime += TimeSpan.FromMinutes(10);
                        UpdateCountUpTimer();
                    },
                    _ => true);
            }
        }

        // タイマーディクリメントコマンド
        private DelegateCommand? _timerDincrementCommand;
        public DelegateCommand TimerDincrementCommand
        {
            get
            {
                return _timerDincrementCommand ??= new DelegateCommand(
                    _ =>
                    {
                        // タイマーを10分ディンクリメントする
                        TimeSpan decrement = _elapsedTime - TimeSpan.FromMinutes(10);
                        _elapsedTime = (decrement > TimeSpan.Zero) ? decrement : TimeSpan.Zero;
                        UpdateCountUpTimer();
                    },
                    _ => true);
            }
        }

        // タイマー開始
        public void StartTimer()
        {
            _isCountUpTimerRunning = true;
            UpdateCommandStates();
            BackgroundColor = "LightBlue";
        }

        // タイマー停止
        private void StopTimer()
        {
            _isCountUpTimerRunning = false;
            UpdateCommandStates();
            BackgroundColor = "White";
        }

        // タイマー表示更新
        public void UpdateCountUpTimer()
        {
            if (_isCountUpTimerRunning)
            {
                _elapsedTime += TimeSpan.FromSeconds(1);
            }
            CountUpTimerText = _elapsedTime.ToString(@"hh\:mm\:ss");
        }

        // コマンドの状態を更新する
        private void UpdateCommandStates()
        {
            TimerStartCommand.RaiseCanExecuteChanged();
            TimerStopCommand.RaiseCanExecuteChanged();
            TimerEditCommand.RaiseCanExecuteChanged();
        }

        // タイマーの背景色
        private string _backgroundColor = "White";
        public string BackgroundColor
        {
            get { return _backgroundColor; }
            set { SetProperty(ref _backgroundColor, value); }
        }
    }
}
