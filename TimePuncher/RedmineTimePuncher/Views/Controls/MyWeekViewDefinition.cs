using LibRedminePower.Extentions;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.ViewModels.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Views.Controls
{
    public class MyWeekViewDefinition : WeekViewDefinition
    {
        public object DataContext
        {
            get { return (object)GetValue(DataContextProperty); }
            set { SetValue(DataContextProperty, value); }
        }

        public static readonly DependencyProperty DataContextProperty =
            DependencyProperty.Register("DataContext", typeof(object), typeof(MyWeekViewDefinition), new PropertyMetadata(null, onDataContextChanged));

        private static void onDataContextChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            var def = target as MyWeekViewDefinition;
            var vm = def.DataContext as InputViewModel;

            BindingOperations.SetBinding(def, DayStartTimeProperty, new Binding("DayStartTime.Value") { Source = vm });
            BindingOperations.SetBinding(def, DayEndTimeProperty, new Binding("DayEndTime.Value") { Source = vm });
            BindingOperations.SetBinding(def, GroupFilterProperty, new Binding("GroupFilter.Value") { Source = vm });
            BindingOperations.SetBinding(def, MinTimeRulerExtentProperty, new Binding("MinTimeRulerExtent.Value") { Source = vm, Mode = BindingMode.TwoWay });
            BindingOperations.SetBinding(def, MinorTickLengthProperty, new Binding("TickLength.Value") { Source = vm });
        }

        public MyWeekViewDefinition() : base()
        {
            GroupHeaderDateStringFormat = "{0:yyyy/MM/dd (ddd)}";
            StretchGroupHeaders = true;
            MajorTickLength = new FixedTickProvider(new DateTimeInterval(0, 1, 0, 0, 0));
            TimerulerMajorTickStringFormat = "{0:HH}:{0:mm}    ";
            TimerulerMinorTickStringFormat = "{0:mm}";
        }

        protected override string FormatVisibleRangeText(IFormatProvider formatInfo, DateTime rangeStart, DateTime rangeEnd, DateTime currentDate)
        {
            return currentDate.ToDateString(formatInfo);
        }

        protected override DateTime GetVisibleRangeStart(DateTime currentDate, CultureInfo culture, DayOfWeek? firstDayOfWeek)
        {
            if (DataContext == null)
                return base.GetVisibleRangeStart(currentDate, culture, firstDayOfWeek);

            var vm = DataContext as InputViewModel;
            return vm.PeriodType.Value.GetStartDate(vm.SelectedDate.Value, vm.Parent.Settings.Calendar);
        }

        protected override DateTime GetVisibleRangeEnd(DateTime currentDate, CultureInfo culture, DayOfWeek? firstDayOfWeek)
        {
            if (DataContext == null)
                return base.GetVisibleRangeEnd(currentDate, culture, firstDayOfWeek);

            var vm = DataContext as InputViewModel;
            return vm.PeriodType.Value.GetEndDate(vm.SelectedDate.Value, vm.Parent.Settings.Calendar);
        }
    }
}
