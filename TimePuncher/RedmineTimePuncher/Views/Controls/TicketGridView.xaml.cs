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
using Telerik.Windows.DragDrop;

namespace RedmineTimePuncher.Views.Controls
{
    /// <summary>
    /// TicketGridView.xaml の相互作用ロジック
    /// </summary>
    public partial class TicketGridView : UserControl
    {
        public TicketGridView()
        {
            InitializeComponent();
        }

        private void RadGridView_RowLoaded(object sender, RowLoadedEventArgs e)
        {
            DragDropManager.SetAllowDrag(e.Row, true);
        }

        private void RadGridView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var radGridView = sender as RadGridView;
            var s = e.OriginalSource as FrameworkElement;
            var parentRow = s.ParentOfType<GridViewRow>();
            if (parentRow != null)
            {
                var dataItem = parentRow.Item;

                if (Keyboard.IsKeyDown(Key.LeftShift) ||
                    Keyboard.IsKeyDown(Key.RightShift) ||
                    Keyboard.IsKeyDown(Key.LeftCtrl) ||
                    Keyboard.IsKeyDown(Key.RightCtrl))
                {
                    radGridView.SelectedItems.Add(dataItem);
                }
                else
                {
                    radGridView.SelectedItems.Clear();
                    radGridView.SelectedItems.Add(dataItem);
                }
            }
        }
    }
}
