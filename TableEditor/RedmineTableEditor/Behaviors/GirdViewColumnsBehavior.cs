using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace RedmineTableEditor.Behaviors
{
    public class GirdViewColumnsBehavior : LibRedminePower.Behaviors.Bases.BehaviorBase<RadGridView>
    {
        public IEnumerable<GridViewBoundColumnBase> Items
        {
            get { return (IEnumerable<GridViewBoundColumnBase>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<GridViewBoundColumnBase>), typeof(GirdViewColumnsBehavior), new UIPropertyMetadata(null, OnItemsChanged));

        private static void OnItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var gridView = (sender as GirdViewColumnsBehavior).AssociatedObject;
            if (gridView != null && e.NewValue != null)
            {
                var items = (IEnumerable<GridViewBoundColumnBase>)e.NewValue;
                gridView.Columns.Clear();
                items.ToList().ForEach(a => gridView.Columns.Add(a));
            }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            if(Items != null)
            {
                AssociatedObject.Columns.Clear();
                Items.ToList().ForEach(a => AssociatedObject.Columns.Add(a));
            }
        }
    }
}
