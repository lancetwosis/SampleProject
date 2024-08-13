using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace RedmineTimePuncher.Converters
{
    public class HasErrorToBorderBrushConverter : LibRedminePower.Converters.Bases.ConverterBase<bool, Brush>
    {
        public override Brush Convert(bool value, object parameter, CultureInfo culture)
        {
            return value ? Brushes.Red : Brushes.Gray;
        }

        public override bool ConvertBack(Brush value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HasErrorToLabelBrushConverter : LibRedminePower.Converters.Bases.ConverterBase<bool, Brush>
    {
        public override Brush Convert(bool value, object parameter, CultureInfo culture)
        {
            return value ? Brushes.Red : Brushes.Black;
        }

        public override bool ConvertBack(Brush value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
