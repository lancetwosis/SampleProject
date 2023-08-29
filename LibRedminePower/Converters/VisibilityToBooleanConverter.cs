using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LibRedminePower.Converters
{
    public class VisibilityToBooleanConverter : LibRedminePower.Converters.Bases.ConverterBase<Visibility, bool>
    {
        public override bool Convert(Visibility value, object parameter, CultureInfo culture)
        {
            return value == Visibility.Visible;
        }

        public override Visibility ConvertBack(bool value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
