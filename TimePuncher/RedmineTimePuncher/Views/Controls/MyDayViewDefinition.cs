using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace RedmineTimePuncher.Views.Controls
{
    public class MyDayViewDefinition : DayViewDefinition
    {
        protected override string FormatVisibleRangeText(IFormatProvider formatInfo, DateTime rangeStart, DateTime rangeEnd, DateTime currentDate)
        {
            return currentDate.ToString("yyyy/MM/dd (ddd)", formatInfo);
        }
    }
}
