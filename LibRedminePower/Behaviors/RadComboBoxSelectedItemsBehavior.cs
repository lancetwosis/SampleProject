using Reactive.Bindings.Notifiers;
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
            var behavior = target as RadComboBoxSelectedItemsBehavior;
            if (behavior.AssociatedObject != null)
            {
                behavior.OnCleanup();
                behavior.OnSetup();
            }
        }

        protected override void OnSetup()
        {
            base.OnSetup();

            if (AssociatedObject != null)
            {
                AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
            }

            if (SelectedItems != null)
            {
                transfer(SelectedItems as IList, AssociatedObject.SelectedItems);
                SelectedItems.CollectionChanged += SelectedItems_CollectionChanged;
            }
        }

        private void SelectedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            transfer(SelectedItems as IList, AssociatedObject.SelectedItems);
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (AssociatedObject != null)
                AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            if (SelectedItems != null)
                SelectedItems.CollectionChanged -= SelectedItems_CollectionChanged;
        }

        private void AssociatedObject_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            transfer(AssociatedObject.SelectedItems, SelectedItems as IList);
        }

        private BusyNotifier nowTransfering = new BusyNotifier();
        private void transfer(IList source, IList target)
        {
            if (nowTransfering.IsBusy || source == null || target == null)
                return;

            using (nowTransfering.ProcessStart())
            {
                target.Clear();
                foreach (var item in source)
                {
                    target.Add(item);
                }
            }
        }
    }
}
