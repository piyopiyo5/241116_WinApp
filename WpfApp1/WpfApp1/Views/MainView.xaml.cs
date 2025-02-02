// --------------------------------------------------
// MainView.xaml.cs
// --------------------------------------------------

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
    /// MainView.xaml の相互作用ロジック
    /// </summary>
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.SelectedItem is TabItem selectedTab)
            {
                if (selectedTab.Tag is string tagValue)
                {
                    if (tagValue == "Auto")
                    {
                        this.SizeToContent = SizeToContent.WidthAndHeight;
                    }
                    else
                    {
                        var dimensions = tagValue.Split(',');
                        if (dimensions.Length == 2 &&
                            int.TryParse(dimensions[0], out int width) &&
                            int.TryParse(dimensions[1], out int height))
                        {
                            this.SizeToContent = SizeToContent.Manual; // 自動調整をオフ
                            this.Width = width;
                            this.Height = height;
                        }
                    }
                }
            }
        }
    }
}
