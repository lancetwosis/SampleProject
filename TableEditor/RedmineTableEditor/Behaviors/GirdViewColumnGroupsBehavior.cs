using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace RedmineTableEditor.Behaviors
{
    public class GirdViewColumnGroupsBehavior : LibRedminePower.Behaviors.Bases.BehaviorBase<RadGridView>
    {
        public IEnumerable<GridViewColumnGroup> Items
        {
            get { return (IEnumerable<GridViewColumnGroup>)GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Items.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsProperty =
            DependencyProperty.Register("Items", typeof(IEnumerable<GridViewColumnGroup>), typeof(GirdViewColumnGroupsBehavior), new UIPropertyMetadata(null, OnItemsChanged));

        private static void OnItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var gridView = (sender as GirdViewColumnGroupsBehavior).AssociatedObject;
            if (gridView != null && e.NewValue != null)
            {
                var items = (IEnumerable<GridViewColumnGroup>)e.NewValue;
                gridView.ColumnGroups.Clear();
                gridView.ColumnGroups.AddRange(items);
            }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            if(Items != null)
            {
                AssociatedObject.ColumnGroups.Clear();
                Items.ToList().ForEach(a => AssociatedObject.ColumnGroups.Add(a));
            }
        }
    }
}
