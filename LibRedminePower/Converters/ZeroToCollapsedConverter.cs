using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LibRedminePower.Converters
{
    public class ZeroToCollapsedConverter : Bases.ConverterBase<object, Visibility>
    {
        public override Visibility Convert(object value, object parameter, CultureInfo culture)
        {
            if (int.TryParse(value.ToString(), out var i))
            {
                return i == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            else if (double.TryParse(value.ToString(), out var d))
            {
                return d == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public override object ConvertBack(Visibility value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
