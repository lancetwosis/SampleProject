using RedmineTimePuncher.Models.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls.ScheduleView;

namespace RedmineTimePuncher.Extentions
{
    public static class IDataSpanExtentions
    {
        public static bool IntersectsWith(this IDateSpan my, TermModel a)
        {
            var b = new DateSpan();
            b.Start = new DateTime(my.Start.Year, my.Start.Month, my.Start.Day).Add(a.Start);
            b.End = b.Start.Add(a.End - a.Start);
            return my.IntersectsWith(b);
        }
    }
}
