using LibRedminePower.Behaviors.Bases;
using System;
using System.Xaml;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using Reactive.Bindings;
using System.Windows;
using Telerik.Windows.Controls;
using TelerikEx.PersistenceProvider;
using LibRedminePower.Extentions;
using LibRedminePower;

namespace TelerikEx.Behaviors
{
    public class RadGridViewPersistenceBehavior : BehaviorBase<RadGridView>
    {
        public GridViewColumnProperties ColumnProperties
        {
            get { return (GridViewColumnProperties)GetValue(GridViewPropertiesProperty); }
            set { SetValue(GridViewPropertiesProperty, value); }
        }
        public static readonly DependencyProperty GridViewPropertiesProperty =
            DependencyProperty.Register(
                nameof(ColumnProperties),
                typeof(GridViewColumnProperties), 
                typeof(RadGridViewPersistenceBehavior), 
                new PropertyMetadata(onLoadGridViewColumns));

        protected override void OnSetup()
        {
            base.OnSetup();
            AssociatedObject.ColumnDisplayIndexChanged += onSaveGridViewColumns;
            AssociatedObject.ColumnWidthChanged += onSaveGridViewColumns;
            AssociatedObject.Sorted += onSaveGridViewColumns;
        }

        protected override void OnCleanup()
        {
            AssociatedObject.ColumnDisplayIndexChanged -= onSaveGridViewColumns;
            AssociatedObject.ColumnWidthChanged -= onSaveGridViewColumns;
            AssociatedObject.Sorted -= onSaveGridViewColumns;
            base.OnCleanup();
        }

        private bool nowLoading;
        private static async void onLoadGridViewColumns(DependencyObject target, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;

            // 起動直後だとAssociatedObjectがnullの場合があるため、10m秒待機する
            await Task.Delay(TimeSpan.FromMilliseconds(10));

            var behavior = target as RadGridViewPersistenceBehavior;
            var columnProps = (GridViewColumnProperties)e.NewValue;

            behavior.nowLoading = true;
            columnProps.Apply(behavior.AssociatedObject);
            behavior.nowLoading = false;
        }

        private void onSaveGridViewColumns(object sender, EventArgs e)
        {
            // Columnsの前回値適用中は行わない
            if (nowLoading == true) return;
            ColumnProperties.Restore(AssociatedObject);
        }
    }
}
