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
using WpfApp1.ViewModels;

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

        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            var dataGrid = sender as DataGrid;

            // 最初の列が存在する場合、降順にソートする
            if (dataGrid != null && dataGrid.Columns.Count > 0)
            {
                // 最初の列（インデックス 0）を降順にソート
                dataGrid.Sorting += (s, args) =>
                {
                    if (args.Column == dataGrid.Columns[0]) // 最初の列の場合
                    {
                        args.Handled = true;
                        // 降順に設定
                        dataGrid.Items.SortDescriptions.Clear();
                        dataGrid.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription(args.Column.SortMemberPath, System.ComponentModel.ListSortDirection.Descending));
                    }
                };

                // 初期状態で降順にソートする
                dataGrid.Items.SortDescriptions.Clear();
                dataGrid.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription(dataGrid.Columns[0].SortMemberPath, System.ComponentModel.ListSortDirection.Descending));
            }
        }

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBox textBox && textBox.DataContext is CountUpTimer timer)
            {
                if (timer.TimerEditCommand.CanExecute(null))
                {
                    timer.TimerEditCommand.Execute(null);
                }
            }
        }
    }
}
