﻿using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class RequestWorkSettingsModel : Bases.SettingsModelBase<RequestWorkSettingsModel>
    {
        [JsonIgnore]
        public ReactivePropertySlim<string> IsBusy { get; set; }

        public MyTracker RequestTracker { get; set; }
        public ReviewIsRequiredSettingModel IsRequired { get; set; }
        public TranscribeSettingModel RequestTranscribe { get; set; }

        [JsonIgnore]
        public List<MyTracker> Trackers { get; set; }

        public RequestWorkSettingsModel()
        {
            IsBusy = new ReactivePropertySlim<string>().AddTo(disposables);
            IsRequired = new ReviewIsRequiredSettingModel().AddTo(disposables);
            RequestTranscribe = new TranscribeSettingModel().AddTo(disposables);
        }

        public async Task SetupAsync(Managers.RedmineManager r)
        {
            try
            {
                IsBusy.Value = Resources.SettingsMsgNowGettingData;

                var trackers = await Task.Run(() => r.Trackers.Value);
                if (!trackers.Any())
                    throw new ApplicationException(Resources.SettingsReviErrMsgNoTrackers);

                Trackers = trackers.Select(t => new MyTracker(t)).ToList();
                Trackers.Insert(0, MyTracker.USE_PARENT_TRACKER);
                RequestTracker = Trackers.FirstOrDefault(RequestTracker);

                var customFields = await Task.Run(() => r.GetCustomFields());
                var boolCustomFields = customFields.Where(c => c.IsBoolFormat()).Select(c => new MyCustomField(c)).ToList();
                IsRequired.Update(boolCustomFields);

                await RequestTranscribe.SetupAsync(r, IsBusy);
            }
            finally
            {
                IsBusy.Value = null;
            }
        }
    }
}