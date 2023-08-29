using Redmine.Net.Api.Types;
using RedmineTimePuncher.Interfaces;
using RedmineTimePuncher.Models.Settings.Bases;
using System;
using System.Collections.ObjectModel;

namespace RedmineTimePuncher.Models.Settings
{
    public class AppointmentSettingsModel : SettingsModelBase<AppointmentSettingsModel>
    {
        public AppointmentMyWorksSettingsModel MyWorks { get; set; } = new AppointmentMyWorksSettingsModel();
        public AppointmentRedmineSettingsModel Redmine { get; set; } = new AppointmentRedmineSettingsModel();
        public AppointmentOutlookSettingsModel Outlook { get; set; } = new AppointmentOutlookSettingsModel();
        public AppointmentTeamsSettingsModel Teams { get; set; } = new AppointmentTeamsSettingsModel();

        public AppointmentSettingsModel()
        {
        }
    }
}
