﻿using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Notifiers;
using RedmineTimePuncher.Extentions;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class TranscribeSettingsViewModel : Bases.SettingsViewModelBase<TranscribeSettingsModel>
    {
        public ReadOnlyReactivePropertySlim<bool> IsEnabledDetectionProcess { get; set; }

        public ReadOnlyReactivePropertySlim<TranscribeSettingViewModel> OpenTranscribe { get; set; }
        public ReadOnlyReactivePropertySlim<TranscribeSettingViewModel> RequestTranscribe { get; set; }

        public TranscribeSettingsViewModel(TranscribeSettingsModel model) : base(model)
        {
            var createTicket = MessageBroker.Default.ToObservable<CreateTicketSettingsModel>();
            var detectionProcess = createTicket.SelectMany(a => a.ObserveProperty(b => b.DetectionProcess));
            IsEnabledDetectionProcess = detectionProcess.SelectMany(x => x.ObserveProperty(dp => dp.IsEnabled))
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);

            OpenTranscribe =
                model.ToReadOnlyViewModel(a => a.OpenTranscribe, 
                a => new TranscribeSettingViewModel(a, true, Resources.SettingsReviTranscribeOpen, Resources.SettingsReviMsgTransOpen)).AddTo(disposables);
            RequestTranscribe =
                model.ToReadOnlyViewModel(a => a.RequestTranscribe, 
                a => new TranscribeSettingViewModel(a, true, Resources.SettingsReviTranscribeRequest, Resources.SettingsReviMsgTransRequest)).AddTo(disposables);
        }
    }
}
