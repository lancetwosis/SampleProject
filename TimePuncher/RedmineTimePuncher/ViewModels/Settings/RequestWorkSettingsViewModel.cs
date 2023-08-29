using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class RequestWorkSettingsViewModel : Bases.SettingsViewModelBase<RequestWorkSettingsModel>
    {
        public ReactivePropertySlim<MyTracker> RequestTracker { get; set; }
        public ObservableCollection<MyTracker> Trackers { get; set; }
        public CustomFieldSettingViewModel IsRequired { get; set; }
        public TranscribeSettingViewModel RequestTranscribe { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public RequestWorkSettingsViewModel(RequestWorkSettingsModel model,
            ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> errorMessage) : base(model)
        {
            RequestTranscribe = new TranscribeSettingViewModel(model.RequestTranscribe, redmine, model.IsBusy).AddTo(disposables);

            redmine.Where(a => a != null).SubscribeWithErr(async r =>
            {
                try
                {
                    await SetupAsync(r, model);
                }
                catch (Exception ex)
                {
                    model.IsBusy.Value = ex.Message;
                }
            }).AddTo(disposables);

            ErrorMessage = new IObservable<string>[] { errorMessage, model.IsBusy }
                .CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // インポートしたら、Viewを読み込み直す
            ImportCommand = ImportCommand.WithSubscribe(async () =>
            {
                await SetupAsync(redmine.Value, model);
            }).AddTo(disposables);
        }

        [JsonIgnore]
        protected CompositeDisposable myDisposables;
        public async Task SetupAsync(RedmineManager r, RequestWorkSettingsModel model)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            await model.SetupAsync(r);

            Trackers = new ObservableCollection<MyTracker>(model.Trackers);
            RequestTracker = model.ToReactivePropertySlimAsSynchronized(m => m.RequestTracker).AddTo(myDisposables);

            IsRequired = new CustomFieldSettingViewModel(model.IsRequired).AddTo(myDisposables);

            RequestTranscribe.Setup(model.RequestTranscribe, model.IsBusy);
        }
    }
}
