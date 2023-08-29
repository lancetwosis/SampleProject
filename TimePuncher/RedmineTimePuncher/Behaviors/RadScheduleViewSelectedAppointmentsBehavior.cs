using LibRedminePower.Behaviors.Bases;
using RedmineTimePuncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Behaviors
{
    public class RadScheduleViewSelectedAppointmentsBehavior : BehaviorBase<RadScheduleView>
    {
        #region SelectedItems Property

        public IEnumerable<MyAppointment> SelectedItems
        {
            get { return (IEnumerable<MyAppointment>)GetValue(SelectedItemsProperty); }
            set { SetValue(SelectedItemsProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemsProperty =
            DependencyProperty.Register("SelectedItems", typeof(IEnumerable<MyAppointment>), typeof(RadScheduleViewSelectedAppointmentsBehavior), new UIPropertyMetadata(null, OnSelectedItemsChanged));

        public static IEnumerable<MyAppointment> GetSelectedAppointments(DependencyObject obj)
        {
            return (IEnumerable<MyAppointment>)obj.GetValue(SelectedItemsProperty);
        }

        private static void OnSelectedItemsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (!ignoreOnChanged)
            {
                var scheduleView = (sender as RadScheduleViewSelectedAppointmentsBehavior).AssociatedObject;
                if (scheduleView != null)
                {
                    var items = (IEnumerable<MyAppointment>)e.NewValue;
                    scheduleView.SelectedAppointments.Clear();
                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            scheduleView.SelectedAppointments.Add(item);
                        }
                    }
                }
            }
        }

        #endregion

        private static bool ignoreOnChanged = false;


        protected override void OnSetup()
        {
            base.OnSetup();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.AppointmentSelectionChanged += AssociatedObject_AppointmentSelectionChanged;
            }
        }

        protected override void OnCleanup()
        {
            base.OnCleanup();

            if (this.AssociatedObject != null)
            {
                this.AssociatedObject.AppointmentSelectionChanged -= AssociatedObject_AppointmentSelectionChanged;
            }
        }

        private void AssociatedObject_AppointmentSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var items = AssociatedObject.SelectedAppointments.Cast<MyAppointment>().ToList();
            if (SelectedItems != items)
            {
                ignoreOnChanged = true;
                SelectedItems = items;
                ignoreOnChanged = false;
            }
        }
    }
}
