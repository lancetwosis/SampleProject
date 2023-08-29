using LibRedminePower.Behaviors.Bases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;

namespace LibRedminePower.Behaviors
{
    public class RadComboBoxNotNullableBehavior : BehaviorBase<RadComboBox>
    {
        protected override void OnSetup()
        {
            base.OnSetup();

            if (AssociatedObject != null)
            {
                AssociatedObject.LostFocus += selectDefaultIfNeeded;
            }
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (this.AssociatedObject != null)
            {
                AssociatedObject.LostFocus -= selectDefaultIfNeeded;
            }
        }

        private void selectDefaultIfNeeded(object sender, RoutedEventArgs e)
        {
            var comboBox = sender as RadComboBox;
            if (comboBox.SelectedItem == null && comboBox.ItemsSource.ToEnumerable().Any())
            {
                comboBox.SelectedIndex = 0;
            }
        }
    }
}
