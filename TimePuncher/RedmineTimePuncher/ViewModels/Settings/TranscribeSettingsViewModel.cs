using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
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

        public TranscribeSettingViewModel OpenTranscribe { get; set; }
        public TranscribeSettingViewModel RequestTranscribe { get; set; }
        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public TranscribeSettingsViewModel(CreateTicketSettingsModel createTicket, TranscribeSettingsModel transcribe,
            ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> errorMessage) : base(transcribe)
        {
            OpenTranscribe = new TranscribeSettingViewModel(redmine, transcribe.IsBusy).AddTo(disposables);
            RequestTranscribe = new TranscribeSettingViewModel(redmine, transcribe.IsBusy).AddTo(disposables);

            redmine.Where(a => a != null).SubscribeWithErr(async r =>
            {
                try
                {
                    await SetupAsync(r, createTicket, transcribe);
                }
                catch (Exception ex)
                {
                    transcribe.IsBusy.Value = ex.Message;
                }
            }).AddTo(disposables);

            var canUseTranscribe = redmine.Select(r => (r != null && !CacheManager.Default.TmpMarkupLang.CanTranscribe()) ? Resources.SettingsReviErrMsgCannotUseTranscribe : null);
            ErrorMessage = new IObservable<string>[] { errorMessage, transcribe.IsBusy, canUseTranscribe }
                .CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // インポートしたら、Viewを読み込み直す
            ImportCommand = ImportCommand.WithSubscribe(async () =>
            {
                var errors = await SetupAsync(redmine.Value, createTicket, transcribe);
                if (errors.Any())
                {
                    MessageBoxHelper.ConfirmWarning(string.Format(Resources.errImport, string.Join(Environment.NewLine, errors)));
                }
            }).AddTo(disposables);
        }

        [JsonIgnore]
        protected CompositeDisposable myDisposables;
        public async Task<List<string>> SetupAsync(RedmineManager r, CreateTicketSettingsModel createTicket, TranscribeSettingsModel transcribe)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            IsEnabledDetectionProcess = createTicket.ObserveProperty(a => a.DetectionProcess.IsEnabled).ToReadOnlyReactivePropertySlim().AddTo(myDisposables);

            var errors = await transcribe.SetupAsync(r, createTicket);

            OpenTranscribe.Setup(transcribe.OpenTranscribe, transcribe.IsBusy);
            RequestTranscribe.Setup(transcribe.RequestTranscribe, transcribe.IsBusy);

            return errors;
        }
    }
}
