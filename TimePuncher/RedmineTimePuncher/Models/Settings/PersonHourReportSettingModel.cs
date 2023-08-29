using FastEnumUtility;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using Microsoft.VisualBasic.CompilerServices;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Enums;
using RedmineTimePuncher.Extentions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class PersonHourReportSettingModel : LibRedminePower.Models.Bases.ModelBase
    {
        public ReportPeriodType Period { get; set; }
        public bool OnTimes { get; set; } = true;
        public bool OnTimesRemaining { get; set; } = true;
        public bool OverTimeAppointment { get; set; } = true;
        public bool ActualTimes { get; set; } = true;

        // RadGridView の「Click here to add new item」機能のために引数なしのコンストラクタが必要
        public PersonHourReportSettingModel()
        {
        }

        public bool IsVisible(PersonHourReportContentType content)
        {
            switch (content)
            {
                case PersonHourReportContentType.OnTimes:
                    return OnTimes;
                case PersonHourReportContentType.OnTimesRemaining:
                    return Period.IsCurrent() && OnTimesRemaining;
                case PersonHourReportContentType.OverTimeAppointment:
                    return OverTimeAppointment; ;
                case PersonHourReportContentType.ActualTimes:
                    return Period.HasActualTimes() && ActualTimes;
                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
