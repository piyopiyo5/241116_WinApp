using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// 定数クラス
    /// </summary>
    internal class Const
    {
        public const int CountUpTimerMax = 6;
        public const int TimerTickInterval = 1;
    }

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

            // Calculatorクラスのインスタンスを生成する
            _calc = new Calculator();

            // 1秒おきにTickイベントを発生させるタイマーを生成する
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(Const.TimerTickInterval)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // カウントアップタイマーのリストを生成する
            CountUpTimers = new ObservableCollection<CountUpTimer>();
            for (int i = 0; i < Const.CountUpTimerMax; i++)
            {
                CountUpTimers.Add(new CountUpTimer("タイマー" + Convert.ToString(i)));
            }

            // 各タイマーに他のタイマーのリストを設定する
            foreach (var timer in CountUpTimers)
            {
                timer.OtherTimers = CountUpTimers.Where(t => t != timer).ToList();
            }

            // 先頭のタイマーをスタートする
            CountUpTimers[0].StartTimer();
        }

        #region 3章のコード
        private string _upperString = string.Empty;
        public string UpperString
        {
            get { return _upperString; }
            private set
            {
                SetProperty(ref _upperString, value);
            }
        }

        private string _inputString = string.Empty;
        public string InputString
        {
            get { return _inputString; }
            set
            {
                if (SetProperty(ref _inputString, value))
                {
                    // 入力された文字列を大文字に変換する
                    UpperString = _inputString.ToUpper();

                    // ClearCommandのCanExecuteChangedイベントを発生させる
                    ClearCommand.RaiseCanExecuteChanged();

                    // 出力ウィンドウに結果を表示する
                    System.Diagnostics.Debug.WriteLine("UpperString=" + UpperString);
                }
            }
        }

        private DelegateCommand? _clearCommand;
        public DelegateCommand ClearCommand
        {
            get
            {
                // コマンドインスタンス_clearCommandを返す。_clearCommandがnullの場合は新しいインスタンスを生成する
                return _clearCommand ??= new DelegateCommand(
                    _ => InputString = "",
                    _ => !string.IsNullOrEmpty(InputString));
            }
        }
        #endregion

        #region 4章のコード
        private Calculator _calc;

        // 割られる数の文字列
        private string _lhs = string.Empty;
        public string Lhs
        {
            get { return _lhs; }
            set
            {
                if (SetProperty(ref _lhs, value))
                {
                    DivCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // 割る数の文字列
        private string _rhs = string.Empty;
        public string Rhs
        {
            get { return _rhs; }
            set
            {
                if (SetProperty(ref _rhs, value))
                {
                    DivCommand.RaiseCanExecuteChanged();
                }
            }
        }

        // 計算結果の文字列
        private string _result = string.Empty;
        public string Result
        {
            get { return _result; }
            private set { SetProperty(ref _result, value); }
        }

        // 割り算コマンド
        private DelegateCommand? _divCommand;
        public DelegateCommand DivCommand
        {
            get
            {
                return _divCommand ??= new DelegateCommand(
                    _ => OnDivision(),
                    _ =>
                    {
                        // 数値変換用後の結果を入れるダミー変数
                        var dummy = 0.0;

                        // 割り算が実行可能か（文字列を数値に変換できるかどうか）を判定する
                        if (!double.TryParse(this.Lhs, out dummy) || !double.TryParse(this.Rhs, out dummy))
                        {
                            return false;
                        }
                        return true;
                    }
                    );
            }
        }

        // 割り算を実行する
        private void OnDivision()
        {
            // 数値変換用の変数
            var lhs = 0.0;
            var rhs = 0.0;

            // 文字列を数値に変換する。変換できない場合は何もしない
            if (!double.TryParse(Lhs, out lhs) || !double.TryParse(Rhs, out rhs))
            {
                return;
            }

            // 割り算を実行する
            _calc.Lhs = lhs;
            _calc.Rhs = rhs;
            _calc.ExecuteDiv();
            Result = _calc.Result.ToString();
        }

        #endregion

        #region 5章のコード
        // ファイルオープンコマンド
        private DelegateCommand? _openFileCommand;
        public DelegateCommand OpenFileCommand
        {
            get
            {
                return _openFileCommand ??= new DelegateCommand(_ => System.Diagnostics.Debug.WriteLine("ファイルを開きます。"));
            }
        }

        #endregion

        #region 時計表示のコード
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

        #endregion

        #region 設定のコード
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
        #endregion
    }

    /// <summary>
    /// カウントアップタイマー
    /// </summary>
    internal class CountUpTimer : NotificationObject
    {
        private TimeSpan _elapsedTime = TimeSpan.Zero; // 経過時間
        private bool _isCountUpTimerRunning = false; // カウントアップタイマーが動作中かどうか

        public CountUpTimer(string TimerName)
        {
            // タイマー名を設定
            _countUpTimerName = TimerName;
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
            private set { SetProperty(ref _countUpTimerText, value); }
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

        // タイマー開始
        public void StartTimer()
        {
            _isCountUpTimerRunning = true;
            UpdateCommandStates();
        }

        // タイマー停止
        private void StopTimer()
        {
            _isCountUpTimerRunning = false;
            UpdateCommandStates();
        }

        // タイマー表示更新
        public void UpdateCountUpTimer()
        {
            if (_isCountUpTimerRunning)
            {
                _elapsedTime += TimeSpan.FromSeconds(1);
                CountUpTimerText = _elapsedTime.ToString(@"hh\:mm\:ss");
            }
        }

        // コマンドの状態を更新する
        private void UpdateCommandStates()
        {
            TimerStartCommand.RaiseCanExecuteChanged();
            TimerStopCommand.RaiseCanExecuteChanged();
        }
    }
}