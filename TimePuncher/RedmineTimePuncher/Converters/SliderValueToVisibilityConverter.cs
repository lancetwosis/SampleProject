using FastEnumUtility;
using LibRedminePower.Extentions;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RedmineTimePuncher.Converters
{
    public class SliderValueToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var sliderValue = (double)values[0];
            var settings = values[1] as ScheduleSettingsModel;
            if (settings == null)
                return Visibility.Visible;

            return settings.GetMinorTickVisibility(sliderValue);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
