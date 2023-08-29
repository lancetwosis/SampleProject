using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Reactive.Bindings.Helpers;
using RedmineTimePuncher.Enums;
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
    public class CreateTicketSettingsViewModel : Bases.SettingsViewModelBase<CreateTicketSettingsModel>
    {
        public CustomFieldSettingViewModel DetectionProcess { get; set; }

        public ReactivePropertySlim<bool> NeedsOutlookIntegration { get; set; }

        public ReactivePropertySlim<MyTracker> OpenTracker { get; set; }
        public ReactivePropertySlim<IdName> OpenStatus { get; set; }
        public CustomFieldSettingViewModel NeedsFaceToFace { get; set; }

        public ReactivePropertySlim<MyTracker> RequestTracker { get; set; }
        public CustomFieldSettingViewModel IsRequired { get; set; }

        public ReactivePropertySlim<MyTracker> PointTracker { get; set; }
        public CustomFieldSettingViewModel SaveReviewer { get; set; }

        public ObservableCollection<IdName> Trackers { get; set; }
        public ObservableCollection<IdName> Statuses { get; set; }

        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }

        public TranscribeSettingsViewModel TranscribeDescription { get; set; }

        public CreateTicketSettingsViewModel(CreateTicketSettingsModel createTicket, TranscribeSettingsModel transcribe,
            ReactivePropertySlim<RedmineManager> redmine, ReactivePropertySlim<string> errorMessage) : base(createTicket)
        {
            TranscribeDescription = new TranscribeSettingsViewModel(createTicket, transcribe, redmine, errorMessage).AddTo(disposables);

            redmine.Where(a => a != null).SubscribeWithErr(async r =>
            {
                try
                {
                    await setUpAsync(r, createTicket);
                }
                catch (Exception ex)
                {
                    createTicket.IsBusy.Value = ex.Message;
                }
            }).AddTo(disposables);

            ErrorMessage = new IObservable<string>[] { errorMessage, createTicket.IsBusy }
                .CombineLatestFirstOrDefault(a => !string.IsNullOrEmpty(a)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            // インポートしたら、Viewを読み込み直す
            ImportCommand = ImportCommand.WithSubscribe(async () =>
            {
                await setUpAsync(redmine.Value, createTicket);

                var errors = await TranscribeDescription.SetupAsync(redmine.Value, createTicket, transcribe);
                if (errors.Any())
                {
                    MessageBoxHelper.ConfirmWarning(string.Format(Resources.errImport, string.Join(Environment.NewLine, errors)));
                }
            }).AddTo(disposables);
        }

        [JsonIgnore]
        protected CompositeDisposable myDisposables;
        private async Task setUpAsync(RedmineManager r, CreateTicketSettingsModel model)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);

            await model.SetupAsync(r);

            DetectionProcess = new CustomFieldSettingViewModel(model.DetectionProcess).AddTo(myDisposables);
            NeedsOutlookIntegration = model.ToReactivePropertySlimAsSynchronized(m => m.NeedsOutlookIntegration).AddTo(myDisposables);

            Trackers = new ObservableCollection<IdName>(model.Trackers);
            Statuses= new ObservableCollection<IdName>(model.Statuses);

            OpenTracker = model.ToReactivePropertySlimAsSynchronized(m => m.OpenTracker).AddTo(myDisposables);
            OpenStatus = model.ToReactivePropertySlimAsSynchronized(m => m.OpenStatus).AddTo(myDisposables);
            NeedsFaceToFace = new CustomFieldSettingViewModel(model.NeedsFaceToFace).AddTo(myDisposables);

            RequestTracker = model.ToReactivePropertySlimAsSynchronized(m => m.RequestTracker).AddTo(myDisposables);
            IsRequired = new CustomFieldSettingViewModel(model.IsRequired).AddTo(myDisposables);

            PointTracker = model.ToReactivePropertySlimAsSynchronized(m => m.PointTracker).AddTo(myDisposables);
            SaveReviewer = new CustomFieldSettingViewModel(model.SaveReviewer).AddTo(myDisposables);
        }
    }
}
