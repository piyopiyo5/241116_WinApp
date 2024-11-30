using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    internal class MainViewModel : NotificationObject
    {
        public CountUpTimer CountUpTimer1;

        public MainViewModel()
        {
            _calc = new Calculator();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // 1秒ごとに更新
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            CountUpTimer1 = new CountUpTimer();

            // 初期時刻を表示
            UpdateClock();
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

        public string CountUpTimer
        {
            get { return CountUpTimer1.CountUpTimerText; }
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
            CountUpTimer1.UpdateCountUpTimer();
        }

        // 時計表示を更新する
        private void UpdateClock()
        {
            ClockText = DateTime.Now.ToString("HH:mm:ss");
        }
        #endregion


    }
}