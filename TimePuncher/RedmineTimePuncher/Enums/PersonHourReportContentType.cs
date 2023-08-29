using LibRedminePower.Attributes;
using LibRedminePower.Converters;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum PersonHourReportContentType
    {
        [LocalizedDescription(nameof(Resources.enumReportColumnOnTimes), typeof(Resources))]
        OnTimes,
        [LocalizedDescription(nameof(Resources.enumReportColumnOnTimesRemaining), typeof(Resources))]
        OnTimesRemaining,
        [LocalizedDescription(nameof(Resources.enumReportColumnOverTimeAppointment), typeof(Resources))]
        OverTimeAppointment,
        [LocalizedDescription(nameof(Resources.enumReportColumnActualTimes), typeof(Resources))]
        ActualTimes,
    }
}
