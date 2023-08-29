using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class TranscribeSettingsModel : Bases.SettingsModelBase<TranscribeSettingsModel>
    {
        [JsonIgnore]
        public ReactivePropertySlim<string> IsBusy { get; set; }

        public TranscribeSettingModel OpenTranscribe { get; set; }
        public TranscribeSettingModel RequestTranscribe { get; set; }

        public TranscribeSettingsModel()
        {
            IsBusy = new ReactivePropertySlim<string>().AddTo(disposables);
            OpenTranscribe = new TranscribeSettingModel().AddTo(disposables);
            RequestTranscribe = new TranscribeSettingModel().AddTo(disposables);
        }

        public async Task<List<string>> SetupAsync(RedmineManager r, CreateTicketSettingsModel createTicket)
        {
            var error1 = await OpenTranscribe.SetupAsync(r, createTicket, IsBusy);
            var error2 = await RequestTranscribe.SetupAsync(r, createTicket, IsBusy);
            return error1.Concat(error2).ToList();
        }
    }
}
