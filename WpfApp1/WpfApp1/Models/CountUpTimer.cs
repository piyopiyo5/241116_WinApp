using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WpfApp1.Models
{
    internal class CountUpTimer : NotificationObject
    {
        // ==================================================
        // プロパティ
        // ==================================================
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        private bool _isRunning = true;
        private string _countUpTimerText = "00:00:00";
        public string CountUpTimerText
        {
            get { return _countUpTimerText; }
            private set
            {
                SetProperty(ref _countUpTimerText, value);
            }
        }

        // ==================================================
        // コマンド
        // ==================================================
        // タイマースタートコマンド
        private DelegateCommand? _timerStartCommand;
        public DelegateCommand TimerStartCommand
        {
            get
            {
                return _timerStartCommand ??= new DelegateCommand(
                    _ =>
                    {
                        _isRunning = true;
                        TimerStartCommand.RaiseCanExecuteChanged();
                        TimerStopCommand.RaiseCanExecuteChanged();
                    },
                    _ => !_isRunning);
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
                        _isRunning = false;
                        TimerStartCommand.RaiseCanExecuteChanged();
                        TimerStopCommand.RaiseCanExecuteChanged();
                    },
                    _ => _isRunning);
            }
        }

        // ==================================================
        // メソッド
        // ==================================================
        // タイマー更新
        public void UpdateCountUpTimer()
        {
            if (_isRunning)
            {
                _elapsedTime += TimeSpan.FromSeconds(1);
                CountUpTimerText = _elapsedTime.ToString(@"hh\:mm\:ss");
            }
        }
    }
}
