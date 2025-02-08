// --------------------------------------------------
// MainView.xaml.cs
// --------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private object _draggedItem;

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

        // ドラッグ開始処理
        private void ListBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            // 押された要素を取得
            DependencyObject originalSource = e.OriginalSource as DependencyObject;

            // ボタンやテキストボックスの場合はドラッグを開始しない
            while (originalSource != null)
            {
                if (originalSource is Button || originalSource is TextBox)
                {
                    return; // ドラッグ開始を中止
                }
                originalSource = VisualTreeHelper.GetParent(originalSource);
            }

            // ドラッグ対象のアイテムを取得
            _draggedItem = GetListBoxItemAtPoint(listBox, e.GetPosition(listBox));
            if (_draggedItem != null)
            {
                DragDrop.DoDragDrop(listBox, _draggedItem, DragDropEffects.Move);
            }
        }

        // ドラッグ中の処理（カーソルの表示調整）
        private void ListBox_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        // ドロップ処理（リストの順番変更）
        private void ListBox_Drop(object sender, DragEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null || _draggedItem == null) return;

            var items = listBox.ItemsSource as ObservableCollection<CountUpTimer>;
            if (items == null) return;

            var droppedItem = GetListBoxItemAtPoint(listBox, e.GetPosition(listBox));
            if (droppedItem == null) return;

            int oldIndex = items.IndexOf(_draggedItem as CountUpTimer);
            int newIndex = items.IndexOf(droppedItem as CountUpTimer);

            if (oldIndex >= 0 && newIndex >= 0 && oldIndex != newIndex)
            {
                items.Move(oldIndex, newIndex);
            }
        }

        // 指定位置のアイテムを取得
        private object GetListBoxItemAtPoint(ListBox listBox, Point position)
        {
            var element = listBox.InputHitTest(position) as UIElement;
            while (element != null && !(element is ListBoxItem))
            {
                element = VisualTreeHelper.GetParent(element) as UIElement;
            }
            return (element as ListBoxItem)?.DataContext;
        }
    }
}
