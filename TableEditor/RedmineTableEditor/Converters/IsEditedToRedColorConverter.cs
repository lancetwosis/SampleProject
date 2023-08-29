using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RedmineTableEditor.Converters
{
    public class IsEditedToRedColorConverter : LibRedminePower.Converters.Bases.ConverterBase<bool, SolidColorBrush>
    {
        public override SolidColorBrush Convert(bool value, object parameter, CultureInfo culture)
        {
            return value ?
                new SolidColorBrush(Colors.Red) :
                new SolidColorBrush(Colors.Black);
        }

        public override bool ConvertBack(SolidColorBrush value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
