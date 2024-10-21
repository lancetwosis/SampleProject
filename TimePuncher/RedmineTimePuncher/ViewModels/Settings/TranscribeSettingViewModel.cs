using LibRedminePower;
using LibRedminePower.Enums;
using LibRedminePower.Extentions;
using LibRedminePower.Helpers;
using LibRedminePower.ViewModels.Bases;
using ObservableCollectionSync;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using RedmineTimePuncher.Models;
using RedmineTimePuncher.Models.Managers;
using RedmineTimePuncher.Models.Settings;
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
using System.Windows.Controls;
using LibRedminePower.ViewModels;
using RedmineTimePuncher.Extentions;
using Reactive.Bindings.Notifiers;
using System.Diagnostics;
using LibRedminePower.Exceptions;

namespace RedmineTimePuncher.ViewModels.Settings
{
    public class TranscribeSettingViewModel : LibRedminePower.ViewModels.Bases.ViewModelBase
    {
        public string Title { get; set; }
        public string Description { get; set; }

        public ReadOnlyReactivePropertySlim<string> ErrorMessage { get; set; }
        public ReadOnlyReactivePropertySlim<bool> IsEnabledDetectionProcess { get; set; }
        public ReadOnlyReactivePropertySlim<List<MyProject>> PossibleProjects { get; set; }
        public ReadOnlyReactivePropertySlim<List<MyProject>> PossibleWikiProjects { get; set; }
        public ReadOnlyReactivePropertySlim<List<MyCustomFieldPossibleValue>> PossibleProcesses { get; set; }
        public ReadOnlyReactivePropertySlim<List<MyTracker>> PossibleTrackers { get; set; }

        public ReactivePropertySlim<bool> IsEnabled { get; set; }
        public ReadOnlyReactivePropertySlim<EditableGridViewModel<TranscribeSettingItemViewModel, TranscribeSettingItemModel>> Items { get; set; }
        public ReactiveCommandSlimToolTip TestCommand { get; set; }

        public TranscribeSettingViewModel(TranscribeSettingModel model, bool isDetectionProcess, string title, string description)
        {
            Title = title;
            Description = description;

            ErrorMessage = new [] {
                CacheTempManager.Default.Message,
                CacheTempManager.Default.MarkupLang.Select(a  => !a.CanTranscribe() ? Resources.SettingsReviErrMsgCannotUseTranscribe : null),
            }.CombineLatest(values => values.FirstOrDefault(a => a != null)).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            var createTicket = MessageBroker.Default.ToObservable<CreateTicketSettingsModel>();
            var detectionProcess = createTicket.SelectMany(a => a.ObserveProperty(b => b.DetectionProcess));
            var detectionProcessCustomField = detectionProcess.SelectMany(a => a.ObserveProperty(b => b.CustomField));

            IsEnabledDetectionProcess =
                !isDetectionProcess ? Observable.Return(false).ToReadOnlyReactivePropertySlim().AddTo(disposables) :
                detectionProcess.SelectMany(x => x.ObserveProperty(dp => dp.IsEnabled)).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            PossibleProjects = CacheTempManager.Default.MyProjects.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            PossibleWikiProjects = CacheTempManager.Default.MyProjectsWiki.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            PossibleProcesses =
                detectionProcessCustomField.CombineLatest(IsEnabledDetectionProcess, (proc, isEnabled) => {
                    return isEnabled ?
                    new List<MyCustomFieldPossibleValue> { TranscribeSettingModel.NOT_SPECIFIED_PROCESS }.Concat(proc.PossibleValues).ToList():
                    new List<MyCustomFieldPossibleValue>();
                }).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            PossibleProcesses.SubscribeWithErr(procs =>
            {
                foreach (var item in model.Items)
                    item.Process = procs?.FirstOrFirst(p => p.Equals(item.Process));
            }).AddTo(disposables);
            PossibleTrackers = CacheTempManager.Default.MyTrackers.Where(a => a != null).Select(a => 
            new List<MyTracker>(new[] { MyTracker.NOT_SPECIFIED }).Concat(a).ToList()).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            IsEnabled = model.ToReactivePropertySlimAsSynchronized(m => m.IsEnabled).AddTo(disposables);
            Items = model.ToReadOnlyViewModel(a => a.Items, 
                a => new EditableGridViewModel<TranscribeSettingItemViewModel, TranscribeSettingItemModel>(a, b => new TranscribeSettingItemViewModel(b), b => b.Model)).AddTo(disposables);

            // Itemsの変更とコレクションの変更を両方監視する
            Items.Select(a => a.CollectionChangedAsObservable())  // 新しいコレクションの変更を監視
                .Switch()                                         // 直前のObservableを破棄して、新しいObservableに切り替え
                .StartWithDefault().SubscribeWithErr(e =>
            {
                if (e?.Action == NotifyCollectionChangedAction.Add)
                {
                    foreach (var item in e.NewItems.Cast<TranscribeSettingItemViewModel>())
                    {
                        item.Process.Value = PossibleProcesses.Value?.FirstOrDefault();
                        item.Project.Value = PossibleProjects.Value?.FirstOrDefault();
                        item.WikiProject.Value = PossibleWikiProjects.Value?.FirstOrDefault();
                    }
                }
            }).AddTo(disposables);

            var selected = Items.SelectMany(a => a.SelectedItem);
            var canTestCommand = new[] {
                selected.Select(a => a == null ? Properties.Resources.SettingsTranscibeMsgSelectItem : null),
                selected.SelectManyIfNotNull(a => a.WikiProject)
                    .Select(a => a == null ? string.Format(Properties.Resources.MsgPleaseSpecifyXXX, Properties.Resources.SettingsReviColWikiProject)  : null),
                selected.SelectManyIfNotNull(a => a.WikiPage)
                    .Select(a => a == null ?  string.Format(Properties.Resources.MsgPleaseSpecifyXXX, Properties.Resources.SettingsReviColWikiPage)  : null),
                selected.SelectManyIfNotNull(a => a.Header)
                    .Select(a => a == null ?  string.Format(Properties.Resources.MsgPleaseSpecifyXXX, Properties.Resources.SettingsReviColHeader)  : null),
            }.CombineLatest().Select(strings => strings.FirstOrDefault(s => s != null)).ToReactiveProperty();
            TestCommand = canTestCommand.ToReactiveCommandToolTipSlim().WithSubscribe(() =>
            {
                var selectedItem = Items.Value.SelectedItem.Value.Model;
                MyWikiPage wiki = null;
                try
                {
                    wiki = CacheTempManager.Default.Redmine.Value.GetWikiPage(selectedItem.WikiProject.Id.ToString(), selectedItem.WikiPage.Title);
                }
                catch
                {
                    throw new ApplicationException(string.Format(Resources.ReviewErrMsgFailedFindWikiPage, selectedItem.WikiPage.Title));
                }

                var lines = wiki.GetSectionLines(CacheTempManager.Default.MarkupLang.Value, selectedItem.Header, selectedItem.IncludesHeader);
                MessageBoxHelper.Input(Resources.ReviewMsgTranscribeFollowings, string.Join(Environment.NewLine, lines.Select(l => l.Text)), true);
            }).AddTo(disposables);

            // Itemsが空の状態で、IsEnabledが有効にされたら、空のModelを追加する。
            IsEnabled.Pairwise().Where(a => !a.OldItem && a.NewItem && !Items.Value.Any()).SubscribeWithErr(_ => model.Items.Add(new TranscribeSettingItemModel())).AddTo(disposables);
        }
    }
}
