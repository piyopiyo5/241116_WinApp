using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    internal class MainViewModel : NotificationObject
    {
        public MainViewModel()
        {
            _calc = new Calculator();

            Timers = new ObservableCollection<TimerViewModel>();

            // 例として3行のタイマーを作成
            for (int i = 0; i < 3; i++)
            {
                Timers.Add(new TimerViewModel($"タイマー{i + 1}"));
            }
        }

        #region 3章のコード
        private string? _upperString;
        public string UpperString
        {
            get { return _upperString; }
            private set
            {
                SetProperty(ref _upperString, value);
            }
        }

        private string? _inputString;
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
                // ClearCommandがnullの場合は新しいインスタンスを生成する
                _clearCommand ??= new DelegateCommand(
                    _ => InputString = "",
                    _ => !string.IsNullOrEmpty(InputString));

                // コマンドを実行する
                return _clearCommand;
            }
        }
        #endregion

        #region 4章のコード
        private Calculator _calc;

        private string _lhs;
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

        private string _rhs;
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

        private string _result;
        public string Result
        {
            get { return _result; }
            private set { SetProperty(ref _result, value); }
        }

        private DelegateCommand _divCommand;
        public DelegateCommand DivCommand
        {
            get
            {
                return _divCommand ??= new DelegateCommand(
                    _ => OnDivision(),
                    _ =>
                    {
                        var dummy = 0.0;
                        if (!double.TryParse(this.Lhs, out dummy))
                        {
                            return false;
                        }
                        if (!double.TryParse(this.Rhs, out dummy))
                        {
                            return false;
                        }
                        return true;
                    }
                    );
            }
        }

        private void OnDivision()
        {
            var lhs = 0.0;
            var rhs = 0.0;
            if (!double.TryParse(Lhs, out lhs))
            {
                return;
            }
            if (!double.TryParse(Rhs, out rhs))
            {
                return;
            }
            _calc.Lhs = lhs;
            _calc.Rhs = rhs;
            _calc.ExecuteDiv();
            Result = _calc.Result.ToString();
        }

        #endregion

        #region 5章のコード
        private DelegateCommand _openFileCommand;
        /// <summary> 
        /// ファイルを開くコマンドを取得します。 
        /// </summary> 
        public DelegateCommand OpenFileCommand
        {
            get
            {
                return _openFileCommand ??= new DelegateCommand(_ => System.Diagnostics.Debug.WriteLine("ファイルを開きます。"));
            }
        }

        #endregion


        // 複数のタイマーを管理するObservableCollection
        public ObservableCollection<TimerViewModel> Timers { get; }

        private DelegateCommand _addTimerCommand;
        public DelegateCommand AddTimerCommand
        {
            get
            {
                return _addTimerCommand ??= new DelegateCommand(
                    _ =>
                    {
                        int newTimerIndex = Timers.Count + 1;
                        Timers.Add(new TimerViewModel($"タイマー{newTimerIndex}"));
                    });
            }
        }
    }

    internal class TimerViewModel : NotificationObject
    {
        private DispatcherTimer _timer;
        private TimeSpan _currentTime = TimeSpan.Zero;
        private bool _isRunning;

        public TimerViewModel(string name)
        {
            Name = name;
            TimerValue = "00:00:00";

            // タイマーの初期設定
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;
        }

        // ユーザーが編集できるタイマー名
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value); // 編集可能
        }

        private string _timerValue;
        public string TimerValue
        {
            get => _timerValue;
            private set
            {
                if (SetProperty(ref _timerValue, value))
                {
                    UpdateCommandStates();
                }
            }
        }

        private DelegateCommand _startCommand;
        public DelegateCommand StartCommand
        {
            get
            {
                return _startCommand ??= new DelegateCommand(
                    _ =>
                    {
                        if (!_isRunning)
                        {
                            _timer.Start();
                            _isRunning = true;
                        }
                    },
                    _ => TimerValue == "00:00:00" && !_isRunning);
            }
        }

        private DelegateCommand _stopCommand;
        public DelegateCommand StopCommand
        {
            get
            {
                return _stopCommand ??= new DelegateCommand(
                    _ =>
                    {
                        if (_isRunning)
                        {
                            _timer.Stop();
                            _isRunning = false;
                        }
                        else
                        {
                            // 停止中に再度停止ボタンを押すとリセット
                            _currentTime = TimeSpan.Zero;
                            TimerValue = "00:00:00";
                        }
                    },
                    _ => TimerValue != "00:00:00" || _isRunning);
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _currentTime = _currentTime.Add(TimeSpan.FromSeconds(1));
            TimerValue = _currentTime.ToString(@"hh\:mm\:ss");
        }

        private void UpdateCommandStates()
        {
            StartCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }
    }
}