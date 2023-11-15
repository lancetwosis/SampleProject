using FastEnumUtility;
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Converters
{
    public class DateTimeToHeaderTextStyleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 2 ||
                !(values[0] is DateTime date) ||
                !(values[1] is DateTime selectedDate))
                return null;

            var style = new Style() { TargetType = typeof(TextBlock) };

            if (selectedDate == date)
            {
                style.Setters.Add(new Setter() { Property = TextBlock.ForegroundProperty, Value = new SolidColorBrush(Colors.White) });
                style.Setters.Add(new Setter() { Property = TextBlock.FontWeightProperty, Value = FontWeights.SemiBold });
            }
            else
            {
                style.Setters.Add(new Setter() { Property = TextBlock.ForegroundProperty, Value = new SolidColorBrush(Colors.Black) });
                style.Setters.Add(new Setter() { Property = TextBlock.FontWeightProperty, Value = FontWeights.Normal });
            }

            if (date == DateTime.Today)
            {
                style.Setters.Add(new Setter() { Property = TextBlock.FontWeightProperty, Value = FontWeights.SemiBold });
                style.Setters.Add(new Setter() { Property = TextBlock.TextDecorationsProperty, Value = TextDecorations.Underline });
            }

            return style;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
