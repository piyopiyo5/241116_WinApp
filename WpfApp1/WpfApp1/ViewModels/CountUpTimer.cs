﻿// -----------------------------------------------------------------
// CountUpTimer.cs
// -----------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.ViewModels
{
    /// <summary>
    /// カウントアップタイマー
    /// </summary>
    internal class CountUpTimer : NotificationObject
    {
        private TimeSpan _elapsedTime = TimeSpan.Zero; // 経過時間
        public bool _isCountUpTimerRunning = false; // カウントアップタイマーが動作中かどうか

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