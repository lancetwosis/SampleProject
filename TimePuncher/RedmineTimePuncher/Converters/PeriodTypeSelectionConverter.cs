using RedmineTimePuncher.Enums;
using RedmineTimePuncher.ViewModels.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Converters
{
    public class PeriodTypeSelectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            var target = FastEnumUtility.FastEnum.Parse<InputPeriodType>(value.ToString());
            var criteria = FastEnumUtility.FastEnum.Parse<InputPeriodType>(parameter.ToString());

            return target == criteria;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
