using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace RedmineTimePuncher.Converters
{
    public class BoolToTextStrikethroughConverter : LibRedminePower.Converters.Bases.ConverterBase<bool, TextDecorationCollection>
    {
        public override TextDecorationCollection Convert(bool value, object parameter, CultureInfo culture)
        {
            return value ? TextDecorations.Strikethrough : null;
        }

        public override bool ConvertBack(TextDecorationCollection value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
