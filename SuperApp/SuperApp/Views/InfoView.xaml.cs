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

namespace SuperApp.Views
{
    /// <summary>
    /// InformationWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class InfoView : Window
    {
        public InfoView(string Text, string Title)
        {
            InitializeComponent();
            InfoText.Text = Text;
            this.Title = Title;
        }

        public InfoView(string Text)
            : this(Text, "Info")
        {
        }

        private void OnOkButtonClick(object sender, RoutedEventArgs e)
        {
            this.Close();  // ウィンドウを閉じる
        }
    }
}
