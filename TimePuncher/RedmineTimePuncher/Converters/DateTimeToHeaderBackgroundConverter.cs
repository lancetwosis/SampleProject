﻿using FastEnumUtility;
using LibRedminePower.Extentions;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Converters
{
    public class DateTimeToHeaderBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3 ||
                !(values[0] is DateTime date) ||
                !(values[1] is CalendarSettingsModel calendar) ||
                !(values[2] is DateTime selectedDate))
                return null;

            if (selectedDate == date)
                return new SolidColorBrush(Office2019Palette.Palette.AccentBackgroundColor);
            else if (calendar.IsWorkingDay(date))
                return TimeEntryType.OnTime.GetColor();
            else
                return TimeEntryType.OverTime.GetColor();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
