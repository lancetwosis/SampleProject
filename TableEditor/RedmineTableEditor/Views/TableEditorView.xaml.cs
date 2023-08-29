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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace RedmineTableEditor.Views
{
    /// <summary>
    /// TableEditorView.xaml の相互作用ロジック
    /// </summary>
    public partial class TableEditorView : UserControl
    {
        public TableEditorView()
        {
            InitializeComponent();
        }

        private void RadGridView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var radGridView = sender as RadGridView;
            var s = e.OriginalSource as FrameworkElement;
            var row = s.ParentOfType<GridViewRow>();
            var cell = s.ParentOfType<GridViewCell>();
            if (row != null && cell != null)
            {
                var dataItem = row.Item;

                if (Keyboard.IsKeyDown(Key.LeftShift) ||
                    Keyboard.IsKeyDown(Key.RightShift) ||
                    Keyboard.IsKeyDown(Key.LeftCtrl) ||
                    Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    if (!radGridView.SelectedCells.Any(a => a.Column == cell.Column))
                        radGridView.SelectedCells.Add(new GridViewCellInfo(dataItem, cell.Column, radGridView));
                }
                else
                {
                    radGridView.SelectedItems.Clear();
                    if (!radGridView.SelectedCells.Any(a => a.Column == cell.Column))
                        radGridView.SelectedCells.Add(new GridViewCellInfo(dataItem, cell.Column, radGridView));
                }
            }
        }

        // 「チケット一覧」と「ピボット」の間の Spliter を上に移動させても、
        // それによって「表示条件」の領域の大きさが変更されないようにする
        private Point _originPoint;
        private void GridSplitter_PreviewMouseDown(object sender, MouseButtonEventArgs e) => _originPoint = e.GetPosition(this);
        private void GridSplitter_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var newPoint = e.GetPosition(this);
                var diff = newPoint.Y - _originPoint.Y;
                if (diff != 0)
                {
                    if (grid.RowDefinitions[1].ActualHeight + diff <= 0)
                        e.Handled = true;
                }
            }
        }
    }
}
