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

namespace WpfApp1.Views
{
    /// <summary>
    /// VersionInfomation.xaml の相互作用ロジック
    /// </summary>
    public partial class VersionInfomationView : Window
    {
        public VersionInfomationView()
        {
            InitializeComponent();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            // 必要に応じて、クリーンアップ処理を追加できます
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // 保存処理などをここに実装
            this.Close();
        }
    }
}
