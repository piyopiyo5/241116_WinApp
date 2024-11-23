using System.ComponentModel;
using WpfApp1.Models;

namespace WpfApp1.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged のメンバ 
        /// <summary> 
        /// プロパティ変更時に発生します。 
        /// </summary> 
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion INotifyPropertyChanged のメンバ 

        /// <summary> 
        /// PropertyChanged イベントを発行します。 
        /// </summary> 
        /// <param name="propertyName">プロパティ名を指定します。</param> 
        protected void RaisePropertyChanged(string propertyName)
        {
            var h = PropertyChanged;
            if (h != null)
                h(this, new PropertyChangedEventArgs(propertyName));
        }

        private string text;
        /// <summary> 
        /// 文字列を取得または設定します。 
        /// </summary> 
        public string Text
        {
            get { return text; }
            set
            {
                if (text != value)
                {
                    text = value;
                    Result = text.ToUpper();
                    RaisePropertyChanged("Text");
                }
            }
        }

        private string result;
        /// <summary> 
        /// 処理結果を取得または設定します。 
        /// </summary> 
        public string Result
        {
            get { return result; }
            set
            {
                if (result != value)
                {
                    result = value;
                    RaisePropertyChanged("Result");
                }
            }
        }

        private DelegateCommand clearCommand;
        /// <summary> 
        /// 文字列をクリアするコマンドを取得します。 
        /// </summary> 
        public DelegateCommand ClearCommand
        {
            get
            {
                if (clearCommand == null)
                    clearCommand = new DelegateCommand(
                    _ => Text = string.Empty,
                    _ => !string.IsNullOrEmpty(Text)
                    );
                return clearCommand;
            }
        }
    }

}
