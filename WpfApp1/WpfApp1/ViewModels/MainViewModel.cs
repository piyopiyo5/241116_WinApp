using System;
using System.Collections.Generic;
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

            // タイマーの初期設定
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += OnTimerTick;

            Timer1 = "00:00:00"; // 初期値
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

        #region タイマー

        private DispatcherTimer _timer;
        private TimeSpan _currentTime = TimeSpan.Zero;
        private bool _isRunning;

        private string _timer1;
        public string Timer1
        {
            get { return _result; }
            private set { SetProperty(ref _result, value); }
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
                    _ => true);
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
                            // 停止中
                            _timer.Stop();
                            _isRunning = false;
                        }
                        else
                        {
                            // 停止中に再度停止ボタンを押すとリセット
                            _currentTime = TimeSpan.Zero;
                            Timer1 = "00:00:00";
                        }
                    },
                    _ => true);
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _currentTime = _currentTime.Add(TimeSpan.FromSeconds(1));
            Timer1 = _currentTime.ToString(@"hh\:mm\:ss");
        }

        #endregion
    }
}