using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.ViewModels
{
    internal class MainViewModel : NotificationObject
    {
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
                    // 入力された文字列を大文字に変換します 
                    UpperString = _inputString.ToUpper();

                    // 出力ウィンドウに結果を表示します 
                    System.Diagnostics.Debug.WriteLine("UpperString=" + UpperString);
                }
            }
        }
    }
}
