using RedmineTimePuncher.Enums;
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
    public class PeriodToIsEnabledConverter : LibRedminePower.Converters.Bases.ConverterBase<ReportPeriodType, bool>
    {
        public override bool Convert(ReportPeriodType value, object parameter, CultureInfo culture)
        {
            var content = FastEnumUtility.FastEnum.Parse<PersonHourReportContentType>(parameter.ToString());
            switch (content)
            {
                case PersonHourReportContentType.OnTimes:
                    return true;
                case PersonHourReportContentType.OnTimesRemaining:
                    return value.IsCurrent();
                case PersonHourReportContentType.OverTimeAppointment:
                    return true;
                case PersonHourReportContentType.ActualTimes:
                    return value.HasActualTimes();
                default:
                    throw new InvalidOperationException();
            }
        }

        public override ReportPeriodType ConvertBack(bool value, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
