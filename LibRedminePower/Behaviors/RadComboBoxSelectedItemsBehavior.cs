using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace LibRedminePower.Behaviors
{
    public class RadComboBoxSelectedItemsBehavior : Bases.BehaviorBase<RadComboBox>
    {

        public INotifyCollectionChanged SelectedItems
        {
            get { return (INotifyCollectionChanged)GetValue(MyPropertyProperty); }
            set { SetValue(MyPropertyProperty, value); }
        }
        public static readonly DependencyProperty MyPropertyProperty = DependencyProperty.Register(
            "SelectedItems",
            typeof(INotifyCollectionChanged),
            typeof(RadComboBoxSelectedItemsBehavior),
            new PropertyMetadata(onSelectedItemsPropertyChanged));

        private static void onSelectedItemsPropertyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (!nowTransfering)
            {
                var comboBox = (target as RadComboBoxSelectedItemsBehavior).AssociatedObject;
                if(comboBox != null)
                {
                    var items = e.NewValue as IList;
                    nowTransfering = true;
                    transfer(items, comboBox.SelectedItems);
                    nowTransfering = false;
                }
            }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            if(AssociatedObject != null)
                AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (AssociatedObject != null)
                AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!nowTransfering)
            {
                nowTransfering = true;
                transfer(AssociatedObject.SelectedItems, SelectedItems as IList);
                nowTransfering = false;
            }
        }

        private static bool nowTransfering = false;

        private static void transfer(IList source, IList target)
        {
            if (source == null || target == null) return;
            target.Clear();
            foreach (var item in source)
            {
                target.Add(item);
            }
        }

    }
}
