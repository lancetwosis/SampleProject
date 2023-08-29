using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Settings.Bases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class AppointmentRedmineSettingsModel : AppointmentResouceIgnoreSettingsModelBase
    {
        public bool IsIgnoreTrackers { get; set; }
        public ObservableCollection<Tracker> IgnoreTrackers { get; set; } = new ObservableCollection<Tracker>();

        public bool IsIgnoreText { get; set; }
        public string IgnoreText { get; set; }

        public override List<MyAppointment> Filter(List<MyAppointment> apos)
        {
            var result = base.Filter(apos);

            if (IsIgnoreTrackers)
                result = result.Where(a => !IgnoreTrackers.Any(b => a.Ticket?.Tracker.Id == b.Id)).ToList();

            if (IsIgnoreText && !string.IsNullOrEmpty(IgnoreText))
            {
                var lines = IgnoreText.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                result = result.Where(a => !lines.Any(b => a.Body.Contains(b))).ToList();
            }

            return result;
        }
    }
}
