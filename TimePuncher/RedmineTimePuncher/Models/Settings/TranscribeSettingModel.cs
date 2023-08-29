using LibRedminePower.Extentions;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using Redmine.Net.Api.Types;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RedmineTimePuncher.Models.Settings
{
    public class TranscribeSettingModel : Bases.SettingsModelBase<TranscribeSettingModel>
    {
        /// <summary>
        /// ReviewTranscribeSettingModel に引数なしのコンストラクタを定義するため static で保持
        /// </summary>
        public static List<MyCustomFieldPossibleValue> PROCESSES { get; set; } = new List<MyCustomFieldPossibleValue>();
        /// <summary>
        /// ReviewTranscribeSettingModel に引数なしのコンストラクタを定義するため static で保持
        /// </summary>
        public static List<MyProject> PROJECTS_ONLY_WIKI_ENABLED { get; set; } = new List<MyProject>();
        /// <summary>
        /// ReviewTranscribeSettingModel に引数なしのコンストラクタを定義するため static で保持
        /// </summary>
        public static RedmineManager REDMINE { get; set; }

        public static MyCustomFieldPossibleValue NOT_SPECIFIED_PROCESS = new MyCustomFieldPossibleValue()
        {
            IsDefault = true,
            Label = LibRedminePower.Properties.Resources.SettingsNotSpecified,
            Value = "-1",
        };

        public bool IsEnabled { get; set; }

        public ObservableCollection<TranscribeSettingItemModel> Items { get; set; }

        public TranscribeSettingModel()
        {
            Items = new ObservableCollection<TranscribeSettingItemModel>();

            this.ObserveProperty(a => a.IsEnabled).SubscribeWithErr(isEnabled =>
            {
                if (isEnabled && !Items.Any())
                {
                    Items.Add(new TranscribeSettingItemModel().AddTo(disposables));
                }
            }).AddTo(disposables);
        }

        [JsonIgnore]
        protected CompositeDisposable myDisposables;
        public async Task<List<string>> SetupAsync(RedmineManager r, CreateTicketSettingsModel createTicket, ReactivePropertySlim<string> isBusy)
        {
            myDisposables?.Dispose();
            myDisposables = new CompositeDisposable().AddTo(disposables);
            try
            {
                isBusy.Value = Resources.SettingsMsgNowGettingData;

                REDMINE = r;
                PROJECTS_ONLY_WIKI_ENABLED = await Task.Run(() => r.GetMyProjectsOnlyWikiEnabled()); ;

                var errors = updateProcesses(createTicket.DetectionProcess);

                createTicket.DetectionProcess.ObserveProperty(a => a.IsEnabled).CombineLatest(createTicket.DetectionProcess.ObserveProperty(a => a.CustomField),
                    (i, v) => (isEnabled: i, customField: v)).Skip(1)
                    .SubscribeWithErr(p =>
                    {
                        updateProcesses(createTicket.DetectionProcess);
                    }).AddTo(myDisposables);

                foreach (var i in Items)
                {
                    await i.SetupAsync(isBusy);
                }

                return errors;
            }
            finally
            {
                isBusy.Value = null;
            }
        }

        private List<string> updateProcesses(ReviewDetectionProcessSettingModel detectionProcess)
        {
            var errors = new List<string>();
            if (detectionProcess.IsEnabled)
            {
                if (detectionProcess.CustomField == null) return errors;

                PROCESSES.Clear();
                PROCESSES.AddRange(detectionProcess.CustomField.PossibleValues);
                foreach (var i in Items)
                {
                    var err = i.ResetProcess();
                    if (err != null)
                        errors.Add(err);
                }
            }
            else
            {
                PROCESSES.Clear();
                foreach (var i in Items)
                {
                    var err = i.ClearProcess();
                    if (err != null)
                        errors.Add(err);
                }
            }

            return errors;
        }

        public async Task SetupAsync(RedmineManager r, ReactivePropertySlim<string> isBusy)
        {
            try
            {
                isBusy.Value = Resources.SettingsMsgNowGettingData;

                REDMINE = r;
                PROJECTS_ONLY_WIKI_ENABLED = await Task.Run(() => r.GetMyProjectsOnlyWikiEnabled()); ;

                foreach (var i in Items)
                {
                    await i.SetupAsync(isBusy);
                }
            }
            finally
            {
                isBusy.Value = null;
            }
        }
    }
}
