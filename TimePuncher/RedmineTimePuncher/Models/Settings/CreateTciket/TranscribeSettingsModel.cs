using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings.CreateTicket
{
    public class TranscribeSettingsModel : Bases.SettingsModelBase<TranscribeSettingsModel>
    {
        public TranscribeSettingModel OpenTranscribe { get; set; } = new TranscribeSettingModel();
        public TranscribeSettingModel SelfTranscribe { get; set; } = new TranscribeSettingModel();
        public TranscribeSettingModel RequestTranscribe { get; set; } = new TranscribeSettingModel();

        public TranscribeSettingsModel()
        { }
    }
}
