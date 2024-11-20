using LibRedminePower.Extentions;
using LibRedminePower.Models;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings.CreateTicket.CustomFields;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket
{
    public class RequestWorkSettingsModel : Bases.SettingsModelBase<RequestWorkSettingsModel>
    {
        public MyTracker RequestTracker { get; set; }
        public ReviewIsRequiredSettingModel IsRequired { get; set; } = new ReviewIsRequiredSettingModel();
        public TranscribeSettingModel RequestTranscribe { get; set; } = new TranscribeSettingModel();

        public RequestWorkSettingsModel()
        {}
    }
}
