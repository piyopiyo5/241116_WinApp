using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WpfApp1.Models
{
    public class DelegateCommand : ICommand
    {
        /// <summary> 
        /// コマンドの実体を保持します。 
        /// </summary> 
        private Action<object> _execute;

        /// <summary> 
        /// コマンドの実行可能判別処理の実態を保持します。 
        /// </summary> 
        private Func<object, bool> _canExecute;

        /// <summary> 
        /// 新しいインスタンスを生成します。 
        /// </summary> 
        /// <param name="execute">コマンドの実体を指定します。</param> 
        public DelegateCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary> 
        /// 新しいインスタンスを生成します。 
        /// </summary> 
        /// <param name="execute">コマンドの実体を指定します。</param> 
        /// <param name="canExecute">コマンドの実行可能判別処理の実体を指定します。</param> 
        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary> 
        /// CanExecuteChanged イベントを発行します。
        ///  /// </summary> 
        public static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #region ICommand のメンバ
        /// <summary> 
        /// コマンドの実行可能判別処理を実行します。 
        /// </summary> 
        /// <param name="parameter">コマンドパラメータを指定します。</param> 
        /// <returns>コマンドが実行可能であるとき true を返します。</returns> 
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute(parameter);
        }

        /// <summary> 
        /// コマンドの実行可能判別条件が変更されたときに発生します。 
        /// </summary> 
        public event System.EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested += value; }
        }

        /// <summary> 
        /// コマンドを実行します。 
        /// </summary> 
        /// <param name="parameter">コマンドパラメータを指定します。</param> 
        public void Execute(object parameter)
        {
            if (_execute != null)
                _execute(parameter);
        }
        #endregion ICommand のメンバ
    }
}
