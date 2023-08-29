using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class AppointmentOutlookSettingsModel : Bases.AppointmentResouceIgnoreSettingsModelBase
    {
        public bool IsEnabled { get; set; } = true;
        public string RefsKeywords { get; set; } = "refs,references,IssueID";
        public bool IsReflectLastInput { get; set; } = true;
    }
}
