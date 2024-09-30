using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace LibRedminePower.Converters
{
    public class VisibilityAnyConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            return values.Select(a =>
            {
                if (a is bool b)
                    return b;
                else if (a is Visibility v)
                    return v == Visibility.Visible;
                else
                    throw new NotSupportedException();
            }).Any(v => v) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
