using RedmineTableEditor.Models.FileSettings;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using LibRedminePower.Extentions;
using System.Windows.Data;
using System.Windows;
using Redmine.Net.Api.Types;
using RedmineTableEditor.Models.TicketFields.Standard;
using RedmineTableEditor.Models;

namespace RedmineTableEditor.Converters
{

    public class AutoCellBackColorConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(x => x == DependencyProperty.UnsetValue)) 
                return AutoBackColorModel.DISABLE_BACKGROUND;

            var setting = (AutoBackColorModel)values[0];
            var redmine = (RedmineManager)values[1];
            var statusId = (int?)values[2];
            var assignedTo = (string)values[3];
            if (setting == null || redmine == null || statusId == null || assignedTo == null)
                return AutoBackColorModel.TRANSPARENT;

            return setting.GetColor(redmine, statusId, assignedTo);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
