using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

            // 複数のタイマーを作成
            for (int i = 0; i < 7; i++)
            {
                Timers.Add(new TimerViewModel($"タイマー{i + 1}"));
            }

            EnableNumLockKeepCommand = new DelegateCommand(_ => EnableNumLockKeep());
            DisableNumLockKeepCommand = new DelegateCommand(_ => DisableNumLockKeep());
            _numLockTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _numLockTimer.Tick += CheckAndEnableNumLock;
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

        private DispatcherTimer _numLockTimer;
        private bool _isNumLockKeepEnabled;

        // コマンド: NumLockKeepを有効にする
        public DelegateCommand EnableNumLockKeepCommand { get; }

        // コマンド: NumLockKeepを無効にする
        public DelegateCommand DisableNumLockKeepCommand { get; }

        private void EnableNumLockKeep()
        {
            _isNumLockKeepEnabled = true;
            _numLockTimer.Start();
        }

        private void DisableNumLockKeep()
        {
            _isNumLockKeepEnabled = false;
            _numLockTimer.Stop();
        }

        private void CheckAndEnableNumLock(object sender, EventArgs e)
        {
            if (_isNumLockKeepEnabled && !IsNumLockActive())
            {
                ToggleNumLock();
            }
        }

        // NumLockの状態を確認する
        private bool IsNumLockActive()
        {
            return (GetKeyState(0x90) & 0x0001) != 0;
        }

        // NumLockをトグルする
        private void ToggleNumLock()
        {
            // NumLockキーを押下
            SendKey(0x90, true);
            // NumLockキーを解放
            SendKey(0x90, false);
        }

        // WinAPI: キーボードの状態を取得する
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        // キー入力をシミュレーション
        private void SendKey(byte keyCode, bool isKeyDown)
        {
            INPUT input = new INPUT
            {
                Type = 1 // INPUT_KEYBOARD
            };
            input.Data.Keyboard = new KEYBDINPUT
            {
                Vk = keyCode,
                Scan = 0,
                Flags = isKeyDown ? 0 : KEYEVENTF_KEYUP,
                Time = 0,
                ExtraInfo = IntPtr.Zero
            };

            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        // WinAPI: キーボード入力をシミュレーション
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        // INPUT構造体
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public int Type;
            public InputUnion Data;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)] public KEYBDINPUT Keyboard;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort Vk;
            public ushort Scan;
            public uint Flags;
            public uint Time;
            public IntPtr ExtraInfo;
        }

        private const int INPUT_KEYBOARD = 1;
        private const uint KEYEVENTF_KEYUP = 0x0002;
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
            private set => SetProperty(ref _timerValue, value);
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
                    _ => true);
            }
        }

        private void OnTimerTick(object? sender, EventArgs e)
        {
            _currentTime = _currentTime.Add(TimeSpan.FromSeconds(1));
            TimerValue = _currentTime.ToString(@"hh\:mm\:ss");
        }
    }
}