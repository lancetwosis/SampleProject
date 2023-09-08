using LibRedminePower.Behaviors.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace TelerikEx.Behaviors
{
    /// <summary>
    /// RadGridView または RadTreeListView で右クリックされたときに SelectedItems を変更するための behavior
    /// </summary>
    public class GridViewSelectionBehavior : BehaviorBase<GridViewDataControl>
    {
        protected override void OnSetup()
        {
            base.OnSetup();
            AssociatedObject.MouseRightButtonUp += mouseRightButtonUp;
        }

        protected override void OnCleanup()
        {
            AssociatedObject.MouseRightButtonUp -= mouseRightButtonUp;
            base.OnCleanup();
        }

        private void mouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var radGridView = sender as GridViewDataControl;
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
