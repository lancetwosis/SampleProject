using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Models.Settings.CreateTicket;
using RedmineTimePuncher.Properties;
using RedmineTimePuncher.ViewModels.Settings.CreateTicket.CustomFields;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings.CreateTicket
{
    public class RequestWorkSettingsViewModel : Bases.SettingsViewModelBase<RequestWorkSettingsModel>
    {
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        public ReadOnlyReactivePropertySlim<List<MyProject>> PossibleProjects { get; set; }
        public ReadOnlyReactivePropertySlim<List<MyTracker>> PossibleTrackers { get; set; }

        public ReactivePropertySlim<MyTracker> RequestTracker { get; set; }
        public ReadOnlyReactivePropertySlim<IsRequiredSettingViewModel> IsRequired { get; set; }
        public ReadOnlyReactivePropertySlim<TranscribeSettingViewModel> RequestTranscribe { get; set; }

        public RequestWorkSettingsViewModel(RequestWorkSettingsModel model) : base(model)
        {
            ErrorMessage = CacheTempManager.Default.Message.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            PossibleProjects = CacheTempManager.Default.MyProjectsWiki.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            PossibleTrackers = CacheTempManager.Default.MyTrackers.Where(a => a != null)
                .Select(a => new List<MyTracker>(new[] { MyTracker.USE_PARENT_TRACKER }).Concat(a).ToList())
                .ToReadOnlyReactivePropertySlim().AddTo(disposables);

            RequestTracker = model.ToReactivePropertySlimAsSynchronized(m => m.RequestTracker).AddTo(disposables);
            IsRequired = model.ToReadOnlyViewModel(a => a.IsRequired, a => new IsRequiredSettingViewModel(a)).AddTo(disposables);
            RequestTranscribe = model.ToReadOnlyViewModel(a => a.RequestTranscribe,
                a => new TranscribeSettingViewModel(a, false, Resources.SettingsReviTranscribeRequest, Resources.SettingsReviMsgTransRequest)).AddTo(disposables);

            PossibleTrackers.Where(a => a != null).Subscribe(trackers => { 
                RequestTracker.Value = trackers.FirstOrFirst(a => a == RequestTracker.Value);
            }).AddTo(disposables);
        }
    }
}
