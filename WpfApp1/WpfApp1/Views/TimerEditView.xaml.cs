using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.ViewModels;

namespace WpfApp1.Views
{
    /// <summary>
    /// TimerEditView.xaml の相互作用ロジック
    /// </summary>
    public partial class TimerEditView : Window
    {
        internal TimerEditView(CountUpTimer countUpTimer)
        {
            InitializeComponent();

            // モーダルウィンドウの DataContext に CountUpTimer を設定
            DataContext = countUpTimer;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 保存処理などをここに実装
            this.Close();
        }

        // 「×」ボタンで閉じる
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // 必要に応じて、クリーンアップ処理を追加できます
        }
    }
}
